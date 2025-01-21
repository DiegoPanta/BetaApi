using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Domain.Entities;
using Interfaces.IRepositories;

namespace Infrastructure.Repositories
{
    public class LoanSimulationRepository : ILoanSimulationRepository
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _context;

        public LoanSimulationRepository()
        {
            _dynamoDbClient = new AmazonDynamoDBClient();
            _context = new DynamoDBContext(_dynamoDbClient);
        }

        public async Task AddAsync(LoanSimulationEntity loanSimulationEntity, CancellationToken cancellationToken)
        {
            await _context.SaveAsync(loanSimulationEntity, cancellationToken);
        }

        public async Task<IEnumerable<LoanSimulationEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            var search = _context.ScanAsync<LoanSimulationEntity>(new List<ScanCondition>());
            var results = await search.GetRemainingAsync(cancellationToken);
            return results;
        }

        public async Task<LoanSimulationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.LoadAsync<LoanSimulationEntity>(id, cancellationToken);
        }
    }

}
