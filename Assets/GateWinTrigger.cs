using UnityEngine;

/// <summary>
/// Triggers the win UI when the player's ball enters the gate trigger.
/// Requires: gate's collider set to IsTrigger=true, and the ball must have a Rigidbody2D.
/// </summary>
public class GateWinTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Win UI controller (on the WinPanel).")]
    public WinUIController winUI;

    [Tooltip("Player launcher that owns the ball. Optional but recommended to disable control after win.")]
    public PlayerLauncher player;

    [Tooltip("Fallback: if true, also accept colliders tagged 'Ball' as the ball.")]
    public bool acceptBallTagFallback = true;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        // Primary check: compare the entering Rigidbody2D to the player's ball
        var rb = other.attachedRigidbody;
        bool isPlayerBall = (player != null && rb == player.ball);

        // Fallback: use tag comparison if configured (set your ball's Tag to "Ball")
        bool isTaggedBall = acceptBallTagFallback && other.CompareTag("Ball");

        if (isPlayerBall || isTaggedBall)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        triggered = true;

        // Stop player control and freeze the ball for a clean finish
        if (player != null)
        {
            player.enabled = false;

            if (player.ball)
            {
                player.ball.linearVelocity = Vector2.zero;
                player.ball.angularVelocity = 0f;
            }

            if (player.arrow) player.arrow.gameObject.SetActive(false);
        }

        if (winUI != null) winUI.Show();
        else Debug.LogWarning("GateWinTrigger: winUI reference is not assigned.");
    }
}
