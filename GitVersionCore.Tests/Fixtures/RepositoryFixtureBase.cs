using System;
using GitVersion;
using LibGit2Sharp;
using Shouldly;

public abstract class RepositoryFixtureBase : IDisposable
{
    public string RepositoryPath;
    public IRepository Repository;
    private Config configuration;

    protected RepositoryFixtureBase(Func<string, IRepository> repoBuilder, Config configuration)
    {
        this.configuration = configuration;
        RepositoryPath = PathHelper.GetTempPath();
        Repository = repoBuilder(RepositoryPath);
        Repository.Config.Set("user.name", "Test");
        Repository.Config.Set("user.email", "test@email.com");
        IsForTrackedBranchOnly = true;
    }

    public bool IsForTrackedBranchOnly { private get; set; }

    public SemanticVersion ExecuteGitVersion(IRepository repository = null)
    {
        var vf = new GitVersionFinder();
        return vf.FindVersion(new GitVersionContext(repository ?? Repository, configuration, IsForTrackedBranchOnly));
    }

    public void AssertFullSemver(string fullSemver, IRepository repository = null)
    {
        ExecuteGitVersion(repository).ToString("f").ShouldBe(fullSemver);
    }

    public virtual void Dispose()
    {
        Repository.Dispose();

        try
        {
            DirectoryHelper.DeleteDirectory(RepositoryPath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to clean up repository path at {0}. Received exception: {1}", RepositoryPath, e.Message);
        }
    }
}