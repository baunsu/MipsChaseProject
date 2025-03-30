using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Player m_player;
    public enum eState : int
    {
        kIdle,
        kHopStart,
        kHop,
        kCaught,
        kNumStates
    }

    private Color[] stateColors = new Color[(int)eState.kNumStates]
   {
        new Color(255, 0,   0),
        new Color(0,   255, 0),
        new Color(0,   0,   255),
        new Color(255, 255, 255)
   };

    // External tunables.
    public float m_fHopTime = 0.2f;
    public float m_fHopSpeed = 6.5f;
    public float m_fScaredDistance = 3.0f;
    public int m_nMaxMoveAttempts = 50;

    // Internal variables.
    public eState m_nState;
    public float m_fHopStart;
    public Vector3 m_vHopStartPos;
    public Vector3 m_vHopEndPos;

    void Start()
    {
        // Setup the initial state and get the player GO.
        m_nState = eState.kIdle;
        m_player = GameObject.FindObjectOfType(typeof(Player)) as Player;
    }

    void HopAway()
    {
        if (m_nState == eState.kCaught) 
            return; 
        
        m_nState = eState.kHop;

        Vector3 moveDir = (transform.position - m_player.transform.position).normalized;

        float randAngle = Random.Range(-90f, 90f);
        Quaternion rotation = Quaternion.Euler(0,0,randAngle);
        Vector3 hopDir = rotation * moveDir;

        m_vHopStartPos = transform.position;
        m_vHopEndPos = m_vHopStartPos + (hopDir * m_fHopSpeed * m_fHopTime);
        m_fHopStart = Time.time;

        transform.up = hopDir;

        m_vHopEndPos.x = Mathf.Clamp(m_vHopEndPos.x, -8.9f, 8.9f);
        m_vHopEndPos.y = Mathf.Clamp(m_vHopEndPos.y, -4.9f, 4.9f);
    }

    void FixedUpdate()
    {
        GetComponent<Renderer>().material.color = stateColors[(int)m_nState];
    }

    void Update()
    {
        if (m_nState == eState.kCaught)
            return;

        float dist_away = Vector3.Distance(m_player.transform.position, transform.position);
       
        if (dist_away <= m_fScaredDistance && m_nState == eState.kIdle)
            HopAway();
        

        if (m_nState == eState.kHop)
        {
            float hop_time = (Time.time - m_fHopStart) / m_fHopTime; 
            transform.position = Vector3.Lerp(m_vHopStartPos, m_vHopEndPos, hop_time);

            if ((Time.time - m_fHopStart) >= m_fHopTime)
            {
                m_nState = eState.kIdle;
                m_fHopStart = Time.time;
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Check if this is the player (in this situation it should be!)
        if (collision.gameObject == GameObject.Find("Player"))
        {
            // If the player is diving, it's a catch!
            if (m_player.IsDiving())
            {
                m_nState = eState.kCaught;
                transform.parent = m_player.transform;
                transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
            }
        }
    }
}