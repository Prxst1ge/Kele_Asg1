# 🍍 KELE Pineapple Tart AR Treasure Hunt

## 📌 Project Overview
This project is a Unity-based Augmented Reality (AR) application designed as an engaging, time-based treasure hunt. The core objective is for the player to physically move around and scan digital ingredient cards to complete the full Pineapple Tart recipe.

## Quick Game Guide
This guide walks you through the step-by-step process of playing the AR Pineapple Tart Treasure Hunt.
### A. Setup & Main Menu

1.  **Launch & Authentication:**
    * Launch the application on your mobile device (compatible with your phone model).
    * Log in to your previous account or create a new account using the secure authentication screen. Your profile is automatically set up in the database upon first login.
2.  **Main Menu Navigation:**
    * Upon logging in, you start at the Main Menu, which offers three options:
        * **About Us:** View details about the KELE company, history, and products.
        * **AR Scanner:** Begin your treasure hunt.
        * **Collections:** View the cards and recipes you have collected/unlocked.

### B. Treasure Hunt Gameplay

3.  **Start Hunt & Timer:**
    * Select the **AR Scanner** button from the Main Menu. You will be directed to the AR scanning page.
    * Once the game initiates, a stopwatch timer (Minutes:Seconds) will appear on the screen and start counting.
    * **Automatic Pause:** If you leave the app or lock your phone, the timer will automatically **pause** and save your exact elapsed time to Firebase, resuming immediately when you return.

4.  **Scanning & Recipe Collection:**
    * Use the device camera to find and scan the hidden ingredient cards (AR targets) in the real-life environment.
    * After successfully scanning a card, a button to **"Add to Recipe"** will appear. Tap this button to confirm and add the item to your digital recipe.

5.  **Component Completion & Progression:**
    * Once all necessary ingredients for the **first component** (e.g., Pineapplepaste) are collected, a virtual **bowl** will appear. Place all ingredients into the bowl to complete that component.
    * You will then be automatically led to the next treasure hunt component (e.g., Pineapple Paste) to repeat the scanning process.

6.  **Final Completion & Reward:**
    * After successfully completing all components of the treasure hunt, the timer will **stop**, and your final total time is recorded.
    * The surprise button will lead you to a reward that can be collected from the store clerk to claim a surprise physical gift.

### C. Post-Game Features

7.  **Viewing Collections:**
    * Return to the Main Menu and select **Collections** to view your unlocked progress. You will see the individual ingredient cards you collected, plus the final completion card unlocked after finishing the entire hunt.
8.  **Replay for Best Time:**
    * Players are able to restart the treasure hunt from the beginning at any time to attempt to get a better total completion time.


## 3. 📱 Platform Support

The application is designed to run on the following platforms:

* **Android Devices:** Requires Android OS version 7.0 (Nougat) or later, and the device must be **ARCore compatible** and capable of running or downloading **Google Play Services for AR**.
* **PC/macOS (Unity Editor):** The core game logic and UI features can be tested directly within the Unity Editor environment.