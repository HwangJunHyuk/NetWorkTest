using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    private NetworkRigidbody2D _pr;
    bool _jumpPressed=false;
    
    private void Awake()
    {
        _pr = GetComponent<NetworkRigidbody2D>();
    }

    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        _pr.Rigidbody.velocity = new Vector2(5 * Input.GetAxisRaw("Horizontal"), _pr.Rigidbody.velocity.y)*Runner.DeltaTime;
        
        if (_jumpPressed)
        {
            Jump();
        }
        
    }
    
    void Jump()
    {
        if (!HasStateAuthority) return;
        _pr.Rigidbody.AddForce(new Vector2(0,850));
        _jumpPressed = false;
    }
    
}
