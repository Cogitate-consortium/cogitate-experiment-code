using Game.Core; // Just Level Stuff

namespace Experiment.Helpers.Game
{
    public static class GameHelpers
    {
        public static TaskType GetTaskType(this LevelConfig levelConfig)
        {
            return levelConfig.isLocalizer ? TaskType.TaskRelevant : TaskType.TaskIrrelevant;
        }
    }

    public enum TaskType { TaskIrrelevant = 0, TaskRelevant = 1 }
}