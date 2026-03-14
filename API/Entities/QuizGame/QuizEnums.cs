namespace API.Entities.QuizGame;

public enum QuestionType
{
    MultipleChoice = 1,
    TrueFalse = 2,
    ShortAnswer = 3
}

public enum QuizMode
{
    Test = 1,
    Game = 2
}

public enum GameSessionStatus
{
    Draft = 1,
    Waiting = 2,
    Live = 3,
    Paused = 4,
    Ended = 5
}

public enum SessionQuestionFlowMode
{
    HostControlled = 1,
    TimedByQuestion = 2
}

public enum ParticipantJoinStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Left = 4
}
