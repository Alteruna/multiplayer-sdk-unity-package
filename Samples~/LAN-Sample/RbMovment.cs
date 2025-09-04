using Alteruna.Multiplayer;
using Alteruna.Multiplayer.Core;
using UnityEngine;

public class RbMovment : Alteruna.Multiplayer.CommunicationBridge
{
    public RigidbodySynchronizable _rbSync;
    
    public float speed = 1f;
    public float maxSpeed = 100f;

    void FixedUpdate()
    {
        float damping = 1f - Mathf.Min(_rbSync.Rigidbody.drag, .999999f);

        _rbSync.velocity = new Vector3( 
            Scale(Input.GetAxisRaw("Horizontal"), _rbSync.velocity.x, damping),
            Scale(Input.GetAxisRaw("Vertical"), _rbSync.velocity.y, damping),
            _rbSync.velocity.z);
    }

    private float Scale(float input, float vel, float damping)
    {
        return Mathf.Clamp(input * speed + vel * damping, -maxSpeed, maxSpeed);
    }

    public override void Possessed(bool isMe, User user)
    {
        enabled = isMe;
    }
}
