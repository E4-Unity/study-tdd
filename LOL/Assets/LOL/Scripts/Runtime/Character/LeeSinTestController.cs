using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOL
{
    public class LeeSinTestController : MonoBehaviour
    {
        LeeSin m_LeeSin;

        bool m_Activated;

        void Awake()
        {
            m_LeeSin = GetComponent<LeeSin>();
        }

        void Start()
        {
            m_LeeSin.SonicWave();
        }

        void Update()
        {
            if (!m_Activated && m_LeeSin.Target is not null)
            {
                m_Activated = true;
                m_LeeSin.SonicWave();
            }
        }
    }
}
