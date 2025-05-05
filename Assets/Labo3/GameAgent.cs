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
    public float SuccessfulJumpReward = 1f;
    public float TargetTouchedPunishment = 10f;
    public float EpisodePassRewardRequirement = 5f;

    private Rigidbody rigidBody;
    private ObstacleSpawner obstacleSpawner;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float targetVelocity;
    private bool onGround = true;

    private void Start() {
        rigidBody = GetComponent<Rigidbody>();
        obstacleSpawner = GetComponent<ObstacleSpawner>();
        initialPosition = transform.localPosition;
        initialRotation = transform.rotation;
        onGround = true;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Obstacle")) {
            SetReward(-TargetTouchedPunishment);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("ground")) onGround = true;
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("ground")) onGround = false;
    }

    public override void OnEpisodeBegin() {
        rigidBody.velocity = Vector3.zero;
        transform.SetLocalPositionAndRotation(initialPosition, initialRotation);
        targetVelocity = Random.Range(TargetVelocityMin, TargetVelocityMax);
        obstacleSpawner.ClearObstacles();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        float continuousJumpInputTolerance = 0.1f;

        obstacleSpawner.TryCreateObstacle(targetVelocity);
        obstacleSpawner.MoveObstacles(targetVelocity);

        // agent is in de lucht
        if (!onGround) {
            AddReward(-AirTimePunishment);
            return;
        }

        // target is voorbij agent
        int passedObstacles = obstacleSpawner.CountObstaclesPassed(transform.position);
        if (passedObstacles > 0) {
            AddReward(passedObstacles * SuccessfulJumpReward);
        }

        // krijgt input actie
        if (actions.ContinuousActions[0] > continuousJumpInputTolerance) {
            rigidBody.AddForce(new Vector3(0, JumpForce, 0), ForceMode.VelocityChange);
            return;
        }

        // genoeg reward = nieuwe episode
        if (GetCumulativeReward() >= EpisodePassRewardRequirement) {
            EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
    }
}
