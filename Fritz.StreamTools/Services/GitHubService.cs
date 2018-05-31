using System;
using System.Threading;
using System.Threading.Tasks;
using Fritz.StreamTools.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fritz.StreamTools.Services
{
    public class GitHubService : IHostedService
    {

				private DateTime _LastUpdate = DateTime.MinValue;

				public GitHubService(GitHubRepository repo, IOptions<GitHubConfiguration> config)
				{
					this.Repository = repo;
					this.Configuration = config.Value;
				}

        public GitHubRepository Repository { get; private set; }
        public GitHubConfiguration Configuration { get; }

				public event EventHandler<GitHubUpdatedEventArgs> Updated = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return MonitorUpdates(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
					return Task.CompletedTask;
        }

        private async Task MonitorUpdates(CancellationToken cancellationToken)
        {

						while (!cancellationToken.IsCancellationRequested) {



							var lastUpdate = await Repository.GetLastCommitTimestamp(Configuration.RepositoryCsv);
							if (lastUpdate <= this._LastUpdate) continue;

							_LastUpdate = lastUpdate;

							var newInfo = await Repository.GetRecentContributors(Configuration.RepositoryCsv);

							if (Updated != null) Updated.Invoke(this, new GitHubUpdatedEventArgs(newInfo, lastUpdate));

							await Task.Delay(300 * 1000);

						}

        }

    }
}
