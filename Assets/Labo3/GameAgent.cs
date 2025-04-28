using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class GameAgent : Agent {
    public float JumpForce = 3.5f;
    public float TargetVelocityMin = 0.05f;
    public float TargetVelocityMax = 0.25f;
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

    public override void OnEpisodeBegin() {
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
        float airTimePunishment = 0.01f;
        float successfulJumpReward = 5f;
        float objectTouchPunishment = 10f;
        float objectDistanceTolerance = 1.5f;
        float onGroundTolerance = 0.1f;
        float continuousJumpInputTolerance = 0.1f;

        Target.localPosition += new Vector3(targetVelocity, 0, 0);

        // agent raakt target aan
        if (Vector3.Distance(transform.localPosition, Target.transform.localPosition) < objectDistanceTolerance) {
            SetReward(-objectTouchPunishment);
            EndEpisode();
            return;
        }

        // agent is in de lucht
        if (Mathf.Abs(transform.localPosition.y - initialPosition.y) > onGroundTolerance) {
            AddReward(-airTimePunishment);
            return;
        }

        // target is voorbij agent
        if (Target.localPosition.x > transform.localPosition.x) {
            SetReward(successfulJumpReward);
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
