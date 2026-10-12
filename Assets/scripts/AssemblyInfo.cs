using System.Runtime.CompilerServices;

// Player의 테스트 전용 internal 멤버(SetInput, CurrentState)를 PlayMode 테스트에만 공개합니다.
[assembly: InternalsVisibleTo("MetalSlug.Tests.PlayMode")]
