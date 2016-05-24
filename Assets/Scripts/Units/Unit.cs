﻿using UnityEngine;
using System.Collections.Generic;
using Interfaces;
using Library;
using UI;
using Units.Controller;
using Units.Skills;

using Event = Define.Event;

namespace Units
{
    public abstract class Unit : MonoBehaviour, IUsesSkills, IControllable
    {
        #region -- VARIABLES --
        // Private member variables
        [SerializeField]
        private UnitNameplate m_Nameplate = null;

        [SerializeField]
        protected ControllerType m_ControllerType;
        protected IController m_Controller;
        protected NavMeshAgent m_NavMeshAgent;
        protected GameObject m_Following;

        protected FiniteStateMachine<DamageState> m_DamageFSM;
        protected FiniteStateMachine<MovementState> m_MovementFSM;

        [SerializeField]
        protected List<Skill> m_Skills;

        [SerializeField]
        protected string m_UnitName;
        [SerializeField]
        protected string m_UnitNickname;

        [SerializeField]
        protected string m_Faction;

        [SerializeField]
        protected float m_BaseHealth = 5f;
        [SerializeField]
        protected float m_HealthGrowth = 2.5f;
        protected float m_MaxHealth;
        protected float m_Health;

        [SerializeField]
        protected float m_BaseMana = 2f;
        [SerializeField]
        protected float m_ManaGrowth = 1f;
        protected float m_MaxMana;
        protected float m_Mana;

        [SerializeField]
        protected float m_BaseDefense = 2f;
        [SerializeField]
        protected float m_DefenseGrowth = 1f;
        protected float m_MaxDefense;
        protected float m_Defense;

        [SerializeField]
        protected int m_BaseLevel;
        protected int m_Level;
        protected float m_Experience;

        protected Vector3 m_TotalVelocity;
        protected Vector3 m_Velocity;

        [SerializeField]
        protected float m_BaseSpeed = 5f;
        [SerializeField]
        protected float m_SpeedGrowth = 0.2f;
        protected float m_Speed;
        protected Moving m_IsMoving;

        protected bool m_CanMoveWithInput;
        #endregion

        #region -- PROPERTIES --
        public NavMeshAgent navMashAgent
        {
            get { return m_NavMeshAgent; }
            set { m_NavMeshAgent = value; }
        }

        public GameObject following
        {
            get { return m_Following; }
            set { m_Following = value; }
        }

        public IController controller
        {
            get { return m_Controller; }
            set { m_Controller = value; }
        }

        public ControllerType controllerType
        {
            get { return m_ControllerType; }
            set { m_ControllerType = value; }
        }

        public FiniteStateMachine<DamageState> damageFSM
        {
            get { return m_DamageFSM; }
            set { m_DamageFSM = value; }
        }

        public FiniteStateMachine<MovementState> movementFSM
        {
            get { return m_MovementFSM; }
            set { m_MovementFSM = value; }
        }

        public List<Skill> skills
        {
            get { return m_Skills; }
            set { m_Skills = value; }
        }

        //string name property
        public string unitName
        {
            get { return m_UnitName; }
            set { m_UnitName = value; }
        }
        //string nickname property
        public string unitNickname
        {
            get { return m_UnitNickname; }
            set { m_UnitNickname = value; }
        }
        public string faction
        {
            get { return m_Faction; }
            set { m_Faction = value; }
        }
        //max mana int property
        public float maxMana
        {
            get { return m_MaxMana; }
            set { m_MaxMana = value; Publisher.self.DelayedBroadcast(Event.UnitMaxManaChanged, this); }
        }

        //mana int property
        public float mana
        {
            get { return m_Mana; }
            set { m_Mana = value; Publisher.self.DelayedBroadcast(Event.UnitManaChanged, this); }
        }

        //Max defense int property
        public float maxDefense
        {
            get { return m_MaxDefense; }
            set { m_MaxDefense = value; }
        }
        //Defense int property
        public float defense
        {
            get { return m_Defense; }
            set { m_Defense = value; }
        }

        //maxhealth int property
        public float maxHealth
        {
            get { return m_MaxHealth; }
            set { m_MaxHealth = value; Publisher.self.DelayedBroadcast(Event.UnitMaxHealthChanged, this); }
        }

        //health int property
        public float health
        {
            get { return m_Health; }
            set { m_Health = value; Publisher.self.DelayedBroadcast(Event.UnitHealthChanged, this); }
        }
        //Experience int property
        public float experience
        {
            get { return m_Experience; }
            set
            {
                m_Experience = value;
                SetLevel();
            }
        }

        //Level int property
        public int level
        {
            get { return m_Level; }
            set { m_Level = value; Publisher.self.DelayedBroadcast(Event.UnitLevelChanged, this); }
        }

        //totalVelecotiy Vector3 property 
        public Vector3 totalVelocity
        {
            get { return m_TotalVelocity; }
            set { m_TotalVelocity = value; }
        }
        //Vector
        public Vector3 velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }


        //Speed int property
        public float speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public Moving isMoving
        {
            get { return m_IsMoving; }
            set { m_IsMoving = value; }
        }

        //canMoveWithInput bool property
        public bool canMoveWithInput
        {
            get { return m_CanMoveWithInput; }
            set { m_CanMoveWithInput = value; }
        }
        #endregion

        #region -- UNITY FUNCTIONS --
        protected virtual void Awake()
        {
            if (m_NavMeshAgent == null)
                m_NavMeshAgent = GetComponent<NavMeshAgent>();

            if (m_Nameplate != null)
            {
                UnitNameplate nameplate = Instantiate(m_Nameplate);
                nameplate.parent = this;
            }

            if (m_Skills == null)
                m_Skills = new List<Skill>();
            else
                for (int i = 0; i < m_Skills.Count; i++)
                {
                    m_Skills[i].skillIndex = i;
                    m_Skills[i].parent = this;
                }

            SetLevel(m_BaseLevel);

            m_MaxHealth = m_BaseHealth;
            m_Health = m_MaxHealth;

            m_MaxMana = m_BaseMana;
            m_Mana = m_MaxMana;

            m_MaxDefense = m_BaseDefense;
            m_Defense = m_MaxDefense;

            //GetComponent<NavMeshAgent>().updateRotation = false;

            SetController();

            m_DamageFSM = new FiniteStateMachine<DamageState>();

            m_DamageFSM.AddTransition(DamageState.Init, DamageState.Idle);
            m_DamageFSM.AddTransitionFromAny(DamageState.Dead);

            m_MovementFSM.Transition(MovementState.Idle);

            m_DamageFSM.Transition(DamageState.Idle);

            Publisher.self.Subscribe(Event.UseSkill, OnUseSkill);
        }

        protected virtual void Start()
        {
            Publisher.self.Broadcast(Event.UnitInitialized, this);
        }

        protected virtual void Update()
        {
            foreach (Skill skill in m_Skills)
                skill.UpdateCooldown();

            if (m_Health <= 0.0f)
                Destroy(gameObject);
        }

        protected virtual void LateUpdate()
        {
            SetMovementFSM();
        }

        protected virtual void OnDestroy()
        {
            m_Controller.UnRegister(this);

            Publisher.self.UnSubscribe(Event.UseSkill, OnUseSkill);
            Publisher.self.Broadcast(Event.UnitDied, this);
        }
        #endregion

        #region -- PRIVATE FUNCTIONS --
        /// <summary>
        /// Use this function to set the unit's level. Do NOT set it manually
        /// </summary>
        /// <param name="a_Level"></param>
        private void SetLevel(int a_Level)
        {
            m_Experience = Mathf.Pow(a_Level, 2) * 10;
            SetLevel();
        }
        /// <summary>
        /// Gets called automatically whenever 'experience' get's changed
        /// </summary>
        private void SetLevel()
        {
            int oldLevel = m_Level;
            int newLevel = (int)Mathf.Sqrt(m_Experience / 10f);

            if (newLevel == m_Level)
                return;

            float oldMaxHealth = maxHealth;
            float oldMaxMana = maxMana;

            maxHealth = m_BaseHealth + (m_HealthGrowth * m_Level);
            maxMana = m_BaseMana + (m_ManaGrowth * m_Level);
            maxDefense = m_BaseDefense + (m_DefenseGrowth * m_Level);
            speed = m_BaseSpeed + (m_SpeedGrowth * m_Level);

            health += maxHealth - oldMaxHealth;
            mana += maxMana - oldMaxMana;

            level = newLevel;

            if (oldLevel < m_Level)
            {
                Publisher.self.Broadcast(Event.UnitLevelUp, this);
                UIAnnouncer.self.FloatingText("Level Up!", transform.position, FloatingTextType.Overhead);
            }
        }
        
        private void SetController()
        {
            m_MovementFSM = new FiniteStateMachine<MovementState>();

            switch (m_ControllerType)
            {
                case ControllerType.GoblinMage:
                    m_Controller = AIController.self;
                    break;
                case ControllerType.Goblin:
                    m_Controller = AIController.self;
                    break;
                case ControllerType.Fortress:
                    m_Controller = UserController.self;
                    break;
                case ControllerType.User:
                    m_CanMoveWithInput = true;
                    m_Controller = UserController.self;
                    break;
            }

            m_Controller.Register(this);
        }

        private void SetMovementFSM()
        {
            if (m_Velocity == Vector3.zero)
                m_MovementFSM.Transition(MovementState.Idle);
            else
                m_MovementFSM.Transition(MovementState.Walking);

            if (m_Velocity.magnitude >= m_Speed / 2.0f)
                m_MovementFSM.Transition(MovementState.Running);
        }
        #endregion

        #region -- EVENTS --
        private void OnUseSkill(Event a_Event, params object[] a_Params)
        {
            IUsesSkills unit = a_Params[0] as IUsesSkills;
            if (unit == null)
                return;

            int skillIndex = (int)a_Params[1];

            if (unit.GetHashCode() != GetHashCode() ||
                m_Skills.Count <= skillIndex ||
                !(m_Skills[skillIndex].remainingCooldown <= 0.0f) ||
                !(m_Skills[skillIndex].skillData.cost <= m_Mana))
                return;

            GameObject newObject = Instantiate(m_Skills[skillIndex].skillPrefab);

            Physics.IgnoreCollision(GetComponent<Collider>(), newObject.GetComponent<Collider>());

            newObject.transform.position = transform.position;
            newObject.GetComponent<IChildable<IUsesSkills>>().parent = this;

            newObject.GetComponent<IMovable>().velocity = new Vector3(
                Mathf.Cos((-transform.eulerAngles.y) * (Mathf.PI / 180)) * newObject.GetComponent<IMovable>().speed,
                0,
                Mathf.Sin((-transform.eulerAngles.y) * (Mathf.PI / 180)) * newObject.GetComponent<IMovable>().speed);

            newObject.GetComponent<ICastable<IUsesSkills>>().skillData = m_Skills[skillIndex].skillData;

            mana -= m_Skills[skillIndex].skillData.cost;

            m_Skills[skillIndex].PutOnCooldown();
        }
        #endregion
    }
}
