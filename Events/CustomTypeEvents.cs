using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Seoner.Events
{
    public class SingleFloatEvent : UnityEvent<float> { }

    public class SingleVector3Event : UnityEvent<Vector3> { }
    public class SingleVector2Event : UnityEvent<Vector2> { }
}

