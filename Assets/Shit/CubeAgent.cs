using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgent : Agent {
    public Transform Target;
    public float SpeedMultiplier = 0.5f;

    public override void OnEpisodeBegin() {
        if (transform.localPosition.y < 0) {
            transform.SetLocalPositionAndRotation(new Vector3(0, 0.5f, 0), Quaternion.identity);
        }

        Target.localPosition = new Vector3(Random.value * 8f - 4f, 0.5f, Random.value * 8f - 4f);
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];

        transform.Translate(controlSignal * SpeedMultiplier);

        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);
        if (distanceToTarget < 1.42f) {
            SetReward(1.0f);
            EndEpisode();
        } else if (transform.localPosition.y < 0) {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}
