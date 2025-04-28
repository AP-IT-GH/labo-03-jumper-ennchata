using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class GameAgent : Agent {
    public float JumpForce = 2f;
    public float TargetVelocityMin = 0.05f;
    public float TargetVelocityMax = 0.25f;
    public float AirTimePunishment = 0.01f;
    public float SuccessfulJumpReward = 5f;
    public float TargetTouchedPunishment = 10f;
    public Transform Target;

    private Rigidbody rigidBody;
    private Vector3 initialPosition;
    private Vector3 targetInitialPosition;
    private float targetVelocity;

    private void Start() {
        rigidBody = GetComponent<Rigidbody>();
        initialPosition = transform.localPosition;
        targetInitialPosition = Target.localPosition;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Obstacle")) {
            SetReward(TargetTouchedPunishment);
            EndEpisode();
        }
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
        float onGroundTolerance = 0.1f;
        float continuousJumpInputTolerance = 0.1f;

        Target.localPosition += new Vector3(targetVelocity, 0, 0);

        // agent is in de lucht
        if (Mathf.Abs(transform.localPosition.y - initialPosition.y) > onGroundTolerance) {
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
