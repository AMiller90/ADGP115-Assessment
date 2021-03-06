﻿using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UI;
using UnityEngine;

namespace Units.Skills
{
    public class Lightning : BaseSkill
    {
        [SerializeField]


        // Use this for initialization
        void Start()
        {
            if (m_Parent == null)
                return;

            AudioManager.self.PlaySound(SoundTypes.Lightning);
            RaycastHit objectHit;

            Vector3 direction = new Vector3(
                        Mathf.Sin(m_Parent.gameObject.transform.eulerAngles.y * (Mathf.PI / 180f)), 0,
                        Mathf.Cos(m_Parent.gameObject.transform.eulerAngles.y * (Mathf.PI / 180f)));

            Physics.SphereCast(
                new Ray(m_Parent.gameObject.transform.position, direction), 0.3f,
                out objectHit);

            Debug.Log(objectHit.transform.gameObject.name);

            if (objectHit.transform.gameObject.GetComponent<Unit>() == null ||
                objectHit.transform.gameObject == m_Parent.gameObject)
                return;

            List<Collider> objectsFound = Physics.OverlapSphere(objectHit.transform.position, 5f).ToList();

            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, m_Parent.gameObject.transform.position);

            int i = 1;
            foreach (Collider objectFound in objectsFound)
            {
                if (objectFound.transform.gameObject.GetComponent<Unit>() == null ||
                    objectFound.transform.gameObject == m_Parent.gameObject)
                    continue;

                objectFound.gameObject.GetComponent<IAttackable>().health -= m_SkillData.damage;

                UIAnnouncer.self.FloatingText(
                m_SkillData.damage,
                objectFound.transform.position,
                FloatingTextType.PhysicalDamage);

                if (objectFound.transform.GetComponent<IStats>() != null &&
                    objectFound.gameObject.GetComponent<IAttackable>().health <= 0)
                    m_Parent.experience += objectFound.transform.GetComponent<IStats>().experience;

                lineRenderer.SetVertexCount(i + 1);
                lineRenderer.SetPosition(i, objectFound.transform.position);

                ++i;

                if (i > 3)
                    break;
            }
        }

        public override string UpdateDescription(SkillData a_SkillData)
        {
            string description = "Does " + a_SkillData.damage + " magic damage";
            return description;
        }
    }
}
