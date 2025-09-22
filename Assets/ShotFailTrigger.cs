using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Triggers the fail UI when the player runs out of shots and attempts to rearm.
/// Monitors the PlayerLauncher's shot count and triggers when ArmAgain would be called with 0 shots.
/// </summary>
public class ShotFailTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Fail UI controller (on the FailPanel).")]
    public FailUIController failUI;

    [Tooltip("Player launcher to monitor for shot count.")]
    public PlayerLauncher player;

    private bool triggered = false;
    private bool fired = false;
    private float stoppedTimer = 0f;

    void Update()
    {
        if (triggered || player == null || failUI == null) return;

        // Track the player's fired state (we need to know when they've shot)
        if (!fired && player.enabled == false)
        {
            fired = true;
            stoppedTimer = 0f;
        }
        else if (!player.enabled)
        {
            fired = true;
        }
        else if (player.enabled)
        {
            fired = false;
            stoppedTimer = 0f;
        }

        // Only check for fail condition if player has fired and is out of shots
        if (fired && player.shotCount <= 0)
        {
            // Check for manual rearm attempt (R key)
            if (Keyboard.current != null && Keyboard.current[player.rearmKey].wasPressedThisFrame)
            {
                TriggerFail();
                return;
            }

            // Check for auto-rearm condition (ball stopped)
            if (player.autoRearmWhenStopped && player.ball != null)
            {
                float speed2 = player.ball.linearVelocity.sqrMagnitude;
                if (speed2 <= player.rearmSpeedThreshold * player.rearmSpeedThreshold)
                {
                    stoppedTimer += Time.deltaTime;
                    if (stoppedTimer >= player.rearmSettleTime)
                    {
                        TriggerFail();
                        return;
                    }
                }
                else
                {
                    stoppedTimer = 0f;
                }
            }
        }
    }

    private void TriggerFail()
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

        if (failUI != null) 
        {
            failUI.Show();
        }
        else 
        {
            Debug.LogWarning("ShotFailTrigger: failUI reference is not assigned.");
        }
    }

    /// <summary>
    /// Public method to manually trigger fail (can be called from other scripts if needed).
    /// </summary>
    public void ForceFailTrigger()
    {
        if (!triggered)
        {
            TriggerFail();
        }
    }

    /// <summary>
    /// Reset the trigger state (useful when restarting level).
    /// </summary>
    public void ResetTrigger()
    {
        triggered = false;
        fired = false;
        stoppedTimer = 0f;
    }
}
