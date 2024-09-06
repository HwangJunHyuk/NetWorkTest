using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkRigidbody2D _rb2;

    private void Awake()
    {
        _rb2 = GetComponent<NetworkRigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        { 
            data.direction.Normalize(); 
            _rb2.Rigidbody.AddForce(200 * data.direction * Runner.DeltaTime);
            //_rb2.Rigidbody.velocity = 5 * data.direction;
        }
    }
}