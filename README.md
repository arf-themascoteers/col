**How to run**

Open the project in Unity and run SampleScene.

**Quick Demo**

https://youtu.be/CZ8wyYlgeC8

**Inner Radius Calculation**
```c#
    float GetInnerRadius()
    {
        float radius = GetAgentRadius();
        return (agents.Length * radius ) / (float)Math.PI;
    }
```

Let,

c = circumference of the inner circle.

radius_agent = radius of agent

radius_inner = radius of inner circle

Then,

c = 2 * π * radius_inner .....(1)

Also, for infinite number of agents, length occupied by each agent on the inner circle without colliding (or just touching) is
the diameter of the agent. Therefore,

c = n * 2 * radius_agent .....(2)

For (1) and (2):

radius_inner = (n * radius_agent) / π

