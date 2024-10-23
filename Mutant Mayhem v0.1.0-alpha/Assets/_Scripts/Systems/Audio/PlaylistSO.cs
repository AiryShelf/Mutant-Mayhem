using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaylistSO_New", menuName = "Audio/PlaylistSO")]
public class PlaylistSO : ScriptableObject
{
    public string playlistName;
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
}
