using System;
using UnityEngine;

// ============================================================
//  ClueNpc.cs
//  A daytime NPC that inherits all existing NpcTalkTo behaviour
//  and adds a stub hook for future clue / AI logic.
//
//  Attach this component INSTEAD of NpcTalkTo on the clue NPC.
//  Assign the same Inspector fields as NpcTalkTo
//  (dialoguePanel, player, interactKey, etc.) — they are inherited.
//
//  Future clue integration:
//  1. Wire a dialogue choice button's OnClick → ClueNpc.OnClueOptionSelected()
//  2. Implement the body of OnClueOptionSelected() when ready.
//     Normal conversations that skip that button remain unaffected.
// ============================================================

public class ClueNpc : NpcTalkTo
{
    // --------------------------------------------------------
    // Inspector — future clue configuration lives here
    // --------------------------------------------------------
    [Header("Clue NPC (Future)")]
    [Tooltip("Optional label shown when this NPC has a clue available. Leave blank for now.")]
    public string clueLabel = "";

    // --------------------------------------------------------
    // Override OpenDialogue to allow future pre-conversation setup
    // (e.g. checking if a clue is ready before the panel appears).
    // Currently passes straight through to the base implementation.
    // --------------------------------------------------------
    public override void OpenDialogue()
    {
        // Base handles: stop walker, show panel, face player, unlock cursor.
        base.OpenDialogue();

        // Placeholder: future logic before the conversation panel appears
        // e.g. check if AI clue is ready, set a dialogue flag, etc.
        // --- nothing here yet ---
    }

    // --------------------------------------------------------
    // CLUE HOOK
    // Wire this to a dialogue choice button in the dialogue panel.
    // When the player clicks "Ask for a Clue", call this method.
    //
    // As long as this is never called, the NPC behaves exactly
    // like every other daytime NPC.
    // --------------------------------------------------------
    public void OnClueOptionSelected()
    {
        // TODO: Implement clue request logic here in the future.
        // This is the only place clue / AI logic will be added.
        // Do NOT modify NpcTalkTo or any other NPC script.

        Debug.Log($"[ClueNpc] '{gameObject.name}' — clue option selected. (Not yet implemented.)");
    }
}
