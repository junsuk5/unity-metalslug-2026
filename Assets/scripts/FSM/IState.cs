namespace FSM
{
    public interface IState
    {
        /// <summary>
        /// 상태에 진입할 때 1회 호출됩니다.
        /// </summary>
        void Enter();

        /// <summary>
        /// 매 프레임(Update)마다 호출됩니다.
        /// </summary>
        void Update();

        /// <summary>
        /// 물리 연산 프레임(FixedUpdate)마다 호출됩니다.
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 상태를 벗어날 때 1회 호출됩니다.
        /// </summary>
        void Exit();
    }
}
