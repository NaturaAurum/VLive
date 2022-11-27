using System.Collections.Generic;
namespace VLive.Runtime.Avatars
{
    [System.Serializable]
    public struct AvatarFileInfo
    {
        public List<string> avatarPathList;
        public int selectedIndex;

        public static AvatarFileInfo Create()
        {
            return new AvatarFileInfo()
            {
                avatarPathList = new List<string>(),
                selectedIndex = -1
            };
        }
    }
}
