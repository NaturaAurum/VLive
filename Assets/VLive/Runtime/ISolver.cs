namespace VLive.Runtime
{
    public interface ISolver<in T> where T : class
    {
        void Solve(T data);
    }

    public interface IPoseSolver : ISolver<PoseData>
    {
        
    }

    public interface IFaceSolver : ISolver<FaceData>
    {
        
    }

    public interface IHandsSolver : ISolver<HandsData>
    {
        
    }
}
