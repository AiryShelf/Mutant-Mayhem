using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaylistSO_New", menuName = "Audio/PlaylistSO")]
public class PlaylistSO : ScriptableObject
{
    public string playlistName;
    [SerializeField] List<SongSO> songListMaster;
    public List<SongSO> songList;
    

    public void ShufflePlaylist()
    {
        for (int i = 0; i < songList.Count; i++)
        {
            SongSO temp = songList[i];
            int randomIndex = Random.Range(i, songList.Count);
            songList[i] = songList[randomIndex];
            songList[randomIndex] = temp;
        }
    }

    public void UnShufflePlaylist()
    {
        //songList = new List<SongSO>(songListMaster);

        for (int i = 0; i < songListMaster.Count; i++)
        {
            songList[i] = songListMaster[i];
        }
    }
}
