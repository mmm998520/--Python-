using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class RollerAgent1 : Agent
{
    public Transform Target;
    Rigidbody rBody;
    public float speed = 10;

    public Transform enemy;
    Rigidbody enemyRBody;
    public float timer;
    public float reTimer;

    public float reward;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        enemyRBody = enemy.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        reward = GetCumulativeReward();
        timer += Time.deltaTime;
        if (this.transform.position.y < 0)
        {
            reTimer += Time.deltaTime;
        }
        else
        {
            reTimer = 0;
        }
    }
    /**
    *訓練前
    */
    public override void OnEpisodeBegin()
    {
        timer = 0;
        //墜落
        // If the Agent fell, zero its momentum
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(Random.value * 8 - 4,
                                      0.5f,
                                      Random.value * 8 - 4);
        //如果碰到Target，將Target隨機移動
        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 8 - 4,
                                      0.5f,
                                      Random.value * 8 - 4);

    }

    /**
    *觀察、收集資料
    **/
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);// Observation Space Size +3 (一個軸+1)
        sensor.AddObservation(this.transform.localPosition);// Observation Space Size +3
        sensor.AddObservation(enemy.transform.localPosition);// Observation Space Size +3

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);// Observation Space Size +1
        sensor.AddObservation(rBody.velocity.z);// Observation Space Size +1
        sensor.AddObservation(enemyRBody.velocity.x);// Observation Space Size +1
        sensor.AddObservation(enemyRBody.velocity.z);// Observation Space Size +1

        sensor.AddObservation(enemy.GetComponent<RollerAgent>().reward);
    }

    /**
     *行動方式
     **/
    public override void OnActionReceived(float[] vectorAction)
    {
        //Agent只有兩種軸向的移動(x,z)
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0]; // Action Space Size+1 (一個軸+1)
        controlSignal.z = vectorAction[1]; // Action Space Size +1
        rBody.AddForce(controlSignal * speed);

        //到達獎勵
        // Reached target and Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        if (distanceToTarget < 1.42f)
        {
            AddReward(5.0f);
            //目標重製
            Target.localPosition = new Vector3(Random.value * 8 - 4,
                                          0.5f,
                                          Random.value * 8 - 4);
        }
        //攻擊獎勵
        if (enemy.position.y < 0f)
        {
            AddReward(0.0065f);
        }

        //掉落懲罰
        // Fell off platform
        if (this.transform.localPosition.y < -5)
        {
            //延遲重製
            if (reTimer > 3)
            {
                AddReward(-2.0f);
                this.transform.localPosition = new Vector3(Random.value * 8 - 4,
                                      0.5f,
                                      Random.value * 8 - 4);
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
                reTimer = 0;
            }
        }

        //時間到重製
        if (timer > 30)
        {
            if (reward > enemy.GetComponent<RollerAgent>().reward)
            {
                AddReward(30f);
            }
            else
            {
                AddReward(-30f);
            }
            EndEpisode();
        }
    }

    /**
     *手動測試
     **/
    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
}
