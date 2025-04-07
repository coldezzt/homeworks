using hw3.TwoPhaseCommit.Shared;

namespace hw3.TwoPhaseCommit.Coordinator.Hubs.Clients;

public interface IPersonClient
{
    Task<bool> Prepare(Person person);
    Task Commit();
    Task Rollback();
}