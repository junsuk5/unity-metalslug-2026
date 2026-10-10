namespace FSM
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        /// <summary>
        /// 초기 상태를 설정하고 Enter()를 실행합니다.
        /// </summary>
        /// <param name="startingState">시작할 상태</param>
        public void Initialize(IState startingState)
        {
            CurrentState = startingState;
            CurrentState?.Enter();
        }

        /// <summary>
        /// 현재 상태를 종료(Exit)하고 새로운 상태로 전이(Enter)합니다.
        /// 동일 상태로의 중복 전이는 무시됩니다.
        /// </summary>
        /// <param name="newState">전이할 새로운 상태</param>
        public void ChangeState(IState newState)
        {
            if (newState == null || CurrentState == newState)
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        /// <summary>
        /// 현재 상태의 Update 로직을 실행합니다.
        /// </summary>
        public void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// 현재 상태의 FixedUpdate 물리 로직을 실행합니다.
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
    }
}
