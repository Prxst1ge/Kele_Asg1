using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions; // Required for ContinueWithOnMainThread
using TMPro; // Required for UI
using UnityEngine.UI;
using UnityEngine.SceneManagement; // To load the AR scene
using System.Collections.Generic;

public class AuthManager : MonoBehaviour
{


    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TextMeshProUGUI loginFeedbackText;


    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public TextMeshProUGUI signupFeedbackText;
    // Firebase variables
    private FirebaseAuth auth;

    void Start()
    {
        // Making sure code is running
        Debug.Log("AuthManager is running");
        // Initialize Firebase Auth
        auth = FirebaseAuth.DefaultInstance;

        // CHECK IF PLAYER HAS ACCOUNT (Auto-Login)
        // If a user is cached from a previous session, go straight to game
        if (auth.CurrentUser != null)
        {
            Debug.Log("User already signed in: " + auth.CurrentUser.UserId);
            LoadGameScene();
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void OnLoginButtonClicked()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        // Basic validation before sending to server
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginFeedbackText.text = "Please enter both email and password.";
            return;
        }

        loginFeedbackText.text = "Logging in...";

        // Call Firebase Login
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // ERROR HANDLING 
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                if (firebaseEx != null)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    string friendlyMessage = GetErrorMessage(errorCode);
                    loginFeedbackText.text = friendlyMessage;
                }
                return;
            }

            // Success!
            Debug.Log("Login Successful!");
            LoadGameScene();
        });
    }

    public void OnSignupButtonClicked()
    {
        string email = signupEmailInput.text;
        string password = signupPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            signupFeedbackText.text = "Please fill in all fields.";
            return;
        }

        signupFeedbackText.text = "Creating Account...";

        // Call Firebase Sign Up
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // ERROR HANDLING
                FirebaseException firebaseEx = task.Exception.GetBaseException() as FirebaseException;
                if (firebaseEx != null)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    string friendlyMessage = GetErrorMessage(errorCode);
                    signupFeedbackText.text = friendlyMessage;
                }
                return;
            }

            // Success!
            Debug.Log("Account Created!");
            // Optional: Create initial database data here (we can add this later)
            LoadGameScene();
        });
    }

    // --- HELPER FUNCTIONS ---

    private void LoadGameScene()
    {
        // Replace "AR_Scene" with the exact name of your game scene
        SceneManager.LoadScene("AR_Scene");
    }

    // This fulfills the "User-friendly error messages" requirement (Week 4 Part 5)
    private string GetErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Please enter an email address.";
            case AuthError.MissingPassword:
                return "Please enter a password.";
            case AuthError.WrongPassword:
                return "Incorrect password. Please try again.";
            case AuthError.InvalidEmail:
                return "Invalid email format.";
            case AuthError.UserNotFound:
                return "Account not found. Please sign up.";
            case AuthError.EmailAlreadyInUse:
                return "This email is already taken.";
            case AuthError.WeakPassword:
                return "Password is too weak. Use at least 6 characters.";
            default:
                return "An unknown error occurred.";
        }
    }
}
