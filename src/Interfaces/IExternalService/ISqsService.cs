using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.IExternalService
{
    public interface ISqsService
    {
        Task SendMessageAsync<T>(string queueUrl, T message);
    }
}
