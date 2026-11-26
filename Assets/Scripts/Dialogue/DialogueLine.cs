using System;
using UnityEngine;

// Uma fala da conversa: quem fala e o que fala. Editavel no Inspector do NPC.
public enum DialogueSpeaker
{
    Npc,
    Player
}

[Serializable]
public class DialogueLine
{
    public DialogueSpeaker speaker = DialogueSpeaker.Npc;

    [TextArea(2, 6)]
    public string text = "";
}
