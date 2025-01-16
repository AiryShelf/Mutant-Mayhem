using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordLink : MonoBehaviour
{
    // Replace this URL with your Discord server invite link
    [SerializeField] private string discordInviteURL = "https://discord.gg/H5FVnQYX";

    // Method to open the Discord link
    public void OpenDiscordInvite()
    {
        Application.OpenURL(discordInviteURL);
    }
}
