using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class GameAgent : Agent {
    public float JumpForce = 3.5f;
    public float TargetVelocityMin = 0.1f;
    public float TargetVelocityMax = 0.35f;
    public float AirTimePunishment = 0.01f;
    public float SuccessfulJumpReward = 5f;
    public float TargetTouchedPunishment = 10f;
    public Transform Target;

    private Rigidbody rigidBody;
    private Vector3 initialPosition;
    private Vector3 targetInitialPosition;
    private float targetVelocity;
    private bool onGround = true;

    private void Start() {
        rigidBody = GetComponent<Rigidbody>();
        initialPosition = transform.localPosition;
        targetInitialPosition = Target.localPosition;
        onGround = true;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Obstacle")) {
            SetReward(TargetTouchedPunishment);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("ground")) onGround = true;
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("ground")) onGround = false;
    }

    public override void OnEpisodeBegin() {
        rigidBody.velocity = Vector3.zero;
        transform.SetPositionAndRotation(initialPosition, Quaternion.identity);
        Target.SetLocalPositionAndRotation(targetInitialPosition, Quaternion.identity);
        targetVelocity = Random.Range(TargetVelocityMin, TargetVelocityMax);
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(targetVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        float continuousJumpInputTolerance = 0.1f;

        Target.localPosition += new Vector3(targetVelocity, 0, 0);

        // agent is in de lucht
        if (!onGround) {
            AddReward(-AirTimePunishment);
            return;
        }

        // target is voorbij agent
        if (Target.localPosition.x > transform.localPosition.x) {
            SetReward(SuccessfulJumpReward);
            EndEpisode();
            return;
        }

        // krijgt input actie
        if (actions.ContinuousActions[0] > continuousJumpInputTolerance) {
            rigidBody.AddForce(new Vector3(0, JumpForce, 0), ForceMode.VelocityChange);
            return;
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
    }
}
