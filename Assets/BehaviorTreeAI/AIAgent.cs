using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

public class AIAgent : MonoBehaviour {
    private AIContext context = new AIContext();
    public AIGraph graph;
    public KoboldCharacterController controller;
    public Rigidbody body;
    public void Start() {
        context["UpdateDelay"] = 0.1f;
        context["controller"] = controller;
        context["rigidbody"] = body;
        context["transform"] = transform;
        context["agent"] = this;
        StartCoroutine(AITicker());
    }

    public void Increment(string variable) {
        if (context.ContainsKey(variable)) {
            context[variable] = (int)context[variable] + 1;
        } else {
            context[variable] = 1;
        }
    }
    public void Decrement(string variable) {
        if (context.ContainsKey(variable)) {
            context[variable] = (int)context[variable] - 1;
        } else {
            context[variable] = 0;
        }
    }

    public IEnumerator AITicker() {
        while (true) {
            yield return new WaitForSeconds((float)context["UpdateDelay"]);
            graph.Tick(context);
        }
    }

    public void OnDestroy() {
        StopAllCoroutines();
    }
}
