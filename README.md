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

This detailed content is perfect. I will now format the entire section using correct Markdown for your README.md.

4. 🎨 Design & Architecture (Markdown Code)
Markdown

## 4. 🎨 Design & Architecture

### Target Audience & Goal

The design process was focused on creating an engaging AR experience integrated with a robust, cloud-based data structure. The primary user goal is to complete the session-based treasure hunt for each component (Biscuit and Pineapple Paste) and successfully save the final, accurate score to the database.

### Database Schema (Data Integrity)

The Firebase Realtime Database is structured to ensure fast retrieval of user data while maintaining clear separation between users and recipe components. The structure supports the current feature set and scalability for future components.

The hierarchy is as follows:

* **`users`**: Stores the unique ID of every user registered via Firebase Authentication.
* **`collection`**: A container for all the major recipe cards a user owns (currently only `PineappleTart`).
* **`PineappleTart`**: Divided into core components: `Biscuit` and `PineapplePaste`.
* **Component Level (`Biscuit`, `PineapplePaste`):**
    * **Ingredient Nodes:** Stores the integer count of collected ingredients (e.g., `Flour_Ingredient: 2`).
    * **`timer` Node:** Stores the session data (`startTime`, `finishTime`, `durationSeconds`, and `isComplete`). This data records how long the full component hunt took.

### Core Design Implementations

This section explains the design choices for key areas of the application:

* **Authentication Flow (Firebase Auth):** The system uses Firebase Authentication to manage user sign-up and login. The design mandates that upon the first successful login, the `DatabaseManager` automatically creates the full nested user profile (`collection/PineappleTart/...`) to ensure that all required ingredient and timer paths exist before the player starts any game session.
* **Session Management (Start & Reset):** To guarantee competitive integrity, the game is designed to be **session-based**. When the player presses 'Start Hunt' or leaves the hunt scene, the `DatabaseManager.StartComponentTimer()` function is called, which ensures the current score and all ingredient counts are immediately reset (0) and a fresh `startTime` is recorded.
* **Completion Trigger (Decoupling):** The final score recording is decoupled from the ingredient collection (the "Add to Recipe" button). The timer only stops when the **physical event** (placing the last item in the bowl) occurs. This is achieved using a **C# Callback (`Action`)** that executes the final score calculation and sets `isComplete: true` in Firebase *before* displaying the completion panel.


## 5. 💻 Tech Stack (Technologies Used)

| Technology | Purpose in Project |
| :--- | :--- |
| **Unity 3D** | The primary cross-platform engine used to build the AR application and manage all game assets, physics, and UI rendering. |
| **C#** | The core programming language used to develop all game logic, handle database communication, and manage complex flow control or to add intereaction like rotations for the game objects. |
| **Firebase Realtime DB** | Selected as the cloud-based backend to store volatile user data, track immutable start times, and synchronize real-time collection counts (scores). |
| **Firebase Authentication** | Used to manage secure user sign-up and login, ensuring all user data stored in the database is correctly scoped by a unique User ID. |
| **AR Foundation** | The Unity package used to enable cross-platform Augmented Reality capabilities, facilitating image tracking for scanning ingredient cards and displaying virtual prefabs. |
| **GitHub** | Used for version control (Git) to track changes, manage the project's history, and collaborate with teammates by allowing synchronized pushing and pulling of code. |