# 🍍 KELE Pineapple Tart AR Treasure Hunt (DDA portion)

## 1. 📌 Project Overview
This project is a Unity-based Augmented Reality (AR) application designed as an engaging, time-based treasure hunt. The core objective is for the player to physically move around and scan digital ingredient cards to complete the full Pineapple Tart recipe.

## 2. Quick Game Guide/Flow
This guide walks you through the step-by-step process of navigating the app.
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
    * **Automatic Pause:** If you leave the app or lock your phone, the timer will automatically **reset**, resetting the timer when resuming the game.

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

### D. Additional Feature

10. **Viewing Collections:**
    * Players are able to to go the online website to check for where they are on the leaderboard and learn more about kele heritage if they want to.


## 3. 📱 Platform Support

The application is designed to run on the following platforms:

* **Android Devices:** Requires Android OS version 7.0 (Nougat) or later, and the device must be **ARCore compatible** and capable of running or downloading **Google Play Services for AR**.
* **PC/macOS (Unity Editor):** The core game logic and UI features can be tested directly within the Unity Editor environment.



## 4. 🎯 What is the Application Catering For

The AR kele application is explicitly designed to cater to the growing demand for **interactive, location-agnostic educational content** focused on **Singaporean Heritage Brands**.

Its core purpose is to transform a passive cultural subject into an Augmented Reality treasure hunt, enabling players to **actively learn more about heritage brands like Kele**—specifically detailing the origin and cultural significance of the Pineapple Tart within that context. The application targets **casual mobile users and students**, offering a fun, competitive session-based experience where success requires physical engagement (AR scanning and interaction) validated by a robust, secure database.

In essence, the application caters for:

* **Gamified Heritage Education:** Making the history and cultural significance of local Singaporean brand kele interactive and accessible.
* **Competitive Engagement:** Providing a reliable platform for timing and scoring (based on the full physical experience), ready for future leaderboard implementation.
* **Data Integrity:** Guaranteeing user progress and score accuracy through resilient cloud-based systems.



## 5.💡 Research

This section documents the commercial and technical precedents that influenced the design and feature set of the AR kele application.

### Game Inspiration

| Inspired Game/App | Element of Inspiration |
| :--- | :--- |
| **Global At-Home AR Easter Egg Hunt App** | **Core AR Scanning Loop & Physical Reward Flow:** Inspired the idea of using the user's phone to **scan for hidden items in the immediate physical environment** (AR scanning on image targets) to track collection progress. The ultimate goal is completing the virtual task to achieve a sense of "real-world" completion (the final recipe). |
| **Prakria Treasure Hunt Game** | **Staged, Multi-Part Collection:** Inspired the concept of a **multi-phase treasure hunt** where players must complete intermediate objectives (the Pineapple Paste stage, the Biscuit stage) before unlocking the final prize or challenge (the complete Pineapple Tart). This ensures structured, prolonged engagement. |

### Feature Adoption & Adaptation

The following features were directly adopted or adapted from these industry examples to meet the specific requirements of data integrity and user experience in our AR environment:

| Implemented Feature | Why Adopted (Industry Standard) |
| :--- | :--- |
| **Two-Phase Collection** (Digital Button + Physical Drop) | **Ensures Physical Immersion:** We adapted the standard AR "collectible" mechanic by adding the **mandatory physical drop** into the bowl *after* the digital button press. This forces the user to engage with the 3D model, ensuring the user experiences both the digital goal-tracking and the physical AR interaction required for completion. |
| **Stage Validation & Card Blocking** | **Prevents Hunt Complication:** Directly inspired by structured treasure hunts (like Prakria). By implementing **Stage Validation** in the `ImageTracker`, we restrict the spawning of irrelevant AR cards, preventing clutter and ensuring the user only focuses on ingredients needed for the current recipe component. |
| **Firebase Transactions** (Count Safety) | **Guarantees Data Integrity (Anti-Spamming):** This technical adoption is critical in any competitive/collectible app. It ensures that players cannot abuse the system by spamming the "Add to Recipe" button to bypass the required count, maintaining the integrity of the competitive scores. |
| **Decoupled Scoring** (Timer Stops on Final Drop) | **Creates Fair, Competitive Metric:** Adapted from competitive timer games. The timer is set to stop only upon the final **physical act** of dropping the last ingredient into the bowl. This accurately times the **entire user experience** (scan, collect, interact), not just the moment the final digital counter reaches its limit. |




## 6. 🎨 Design & Architecture

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





## 7. 📝 Content Displayed

This section details the educational, cultural, and visual assets used to create the Augmented Reality recipe experience, alongside the crucial user feedback mechanisms and internal data representation.

### 1. Educational and Cultural Content

The application delivers specific cultural and historical facts through its user interfaces:

* **Brand Origin (About Us):** The dedicated **About Us** page provides historical context and details regarding the **origin and cultural significance of the Kele Pineapple Tart** brand, offering deeper engagement with the product's heritage.
* **Recipe Components (Interactive Facts: Flavor Contribution):** The specific role of each ingredient in contributing to the flavor and texture is displayed as interactive facts within the AR scene:
    * **Pineapple Paste:** Highlights how Pineapple provides the sweet/tart base, Sugar controls sweetness, and Lemon balances the flavor of the jam filling.
    * **Biscuit Dough:** Highlights how Butter provides richness and tenderness, Flour forms the structural base, and Water ensures the melt-in-your-mouth texture.

### 2. Collections Page (Tracking and Rewards)

The Collections Page provides persistent visual feedback on the user's progress:

* **Completion Visuals:** The user can see all ingredient cards they have collected (those with a count $\ge 1$) displayed as **unlocked** (GreyLockPanel inactive) after completing the respective treasure hunt components.
* **Special Card Reward:** A key feature is the display of the **Secret Card**. This final reward is unlocked only when the complex condition in `DatabaseManager.AreAllIngredientsCollected()` is met, confirming the player has finished **both** the Pineapple Paste and Biscuit hunts.

### 3. Visual and Interactive Assets

The AR scene and game UI rely on the following critical visual and interactive assets:

* **Competitive Stopwatch UI:** A persistent, running stopwatch is displayed throughout the active treasure hunt session. This is essential for competitive play, allowing the user to **track their elapsed time** in real-time before the final score is recorded upon component completion.
* **Image Targets (AR Cards):** Custom-designed physical cards used to trigger the AR experience.
* **3D Models:** Collectible Ingredients (draggable and rotatable), the Mixing Containers (Bowl Model), and the Final Products (Paste/Dough models).
* **User Feedback and Error Content:** The system provides explicit, user-friendly feedback for various states, including **Authentication Errors** and **In-Game Logic Errors** (e.g., the Invalid Card Popup).

### 4. Database Representation (Internal Content State)

* **Ingredient Status:** Collection is displayed internally as an **Integer Count** (e.g., `Flour_Ingredient: 2`), ensuring precise inventory tracking.
* **Competitive Scoring:** Session duration is stored as `durationSeconds` (a long integer), which is the primary metric for competitive play.




## 8. 💻 Tech Stack (Technologies Used)

| Technology | Purpose in Project |
| :--- | :--- |
| **Unity 3D** | The primary cross-platform engine used to build the AR application and manage all game assets, physics, and UI rendering. |
| **C#** | The core programming language used to develop all game logic, handle database communication, and manage complex flow control or to add intereaction like rotations for the game objects. |
| **Firebase Realtime DB** | Selected as the cloud-based backend to store volatile user data, track immutable start times, and synchronize real-time collection counts (scores). |
| **Firebase Authentication** | Used to manage secure user sign-up and login, ensuring all user data stored in the database is correctly scoped by a unique User ID. |
| **AR Foundation** | The Unity package used to enable cross-platform Augmented Reality capabilities, facilitating image tracking for scanning ingredient cards and displaying virtual prefabs. |
| **GitHub** | Used for version control (Git) to track changes, manage the project's history, and collaborate with teammates by allowing synchronized pushing and pulling of code.





## 9. ✨ Key Features

### Existing Features |


 **User-friendly Auth Errors**  **Secure User Authentication & Profile Integrity:** Implements Firebase Authentication... Provides user-friendly feedback and error handling for common issues like weak passwords or invalid credentials.  

**In-Game Visual Timer Exists**  **(Implied by Multiple Features):** The `StopwatchUI` reference in your stage managers, combined with the Session Management features, shows a timer that is active in the game UI. A stopwatch that will start counting the moment the player has hit the play button and end when the player has finsished the treasure hunt.

 **Final Score is Recorded in DB**  **Completion Trigger (Decoupled Scoring):** The timer is stopped, and the final score is calculated only when the physical mixing is complete (`CheckMixingComplete` / `CheckBowlComplete`). The database records the `durationSeconds` and sets `isComplete` to `true` upon completion and sends it to the realtime database from firebase. 

* **Secure User Authentication & Profile Integrity:** Implements Firebase Authentication for sign-up/login. The system mandates that upon the first successful login, the `DatabaseManager` automatically creates the full nested user profile in Firebase, ensuring all required ingredient and timer paths exist.

* **Count-based Data Collection:** The **`AddIngredientToDatabase()`** function uses Firebase Transactions to safely and reliably increment the ingredient count (e.g., `Pineapple_Ingredient: 1` to `2`) and prevents over-counting beyond the required amount.

* **Stage Validation:** The **`ImageTracker`** uses the **`PineapplePasteStageManager`** and **`BiscuitStageManager`** to perform immediate client-side validation, blocking the spawning of prefabs for ingredient cards that are not valid for the currently active stage.

* **Core AR Interaction (Drag, Drop, and Rotation):**
    * The **`IngredientDraggable.cs`** script implements custom drag functionality, allowing users to physically manipulate 3D ingredient models in the AR space.
    * The **`IngredientController.cs`** provides the **`ToggleRotate()`** method and continuous rotation logic (`Update()` method) to enhance the visual interactivity of the 3D models.

* **Two-Phase Ingredient Flow:** Implements a strict, two-stage process for ingredient collection:
    1.  **Digital Collection:** The "Add to Recipe" button increments the database count and updates a local counter.
    2.  **Physical Completion:** The ingredient must be **physically dragged** into the virtual bowl to progress the local `dropped` count, triggering the next step.

* **Completion Trigger (Decoupled Scoring):** The timer is stopped, and the final score is calculated **only** when the physical mixing is complete (`CheckMixingComplete` / `CheckBowlComplete`). This ensures the timer reflects the full process and is decoupled from the initial button press.

* **Visual Collection Feedback:** The `LoadUserCollection` function reads the count data from Firebase to visually unlock/hide lock panels on collection UI elements when the count is greater than zero.

* **On-Demand UI Control:** Implements the **`CardUIController`** to allow users to toggle display of supplementary information/translation panels on the AR cards.

* **Firebase Initialization & Dependency Check:** Uses **`FirebaseInit.cs`** to check for and fix all necessary Firebase dependencies on application start, ensuring the environment is stable for all subsequent database and authentication calls.

* **Dual Stage Game Flow:** Implements two separate, complete recipe stages (Pineapple Paste and Biscuit) managed by dedicated scripts (**`PineapplePasteStageManager.cs`** and **`BiscuitStageManager.cs`**), each with unique required ingredients and local progress tracking.

* **Scene Navigation:** The **`MenuController.cs`** provides robust methods for navigating the application (AR Scanner, Collection, About Us) and handling application exit.





## 10. 🧪 Testing & Quality Assurance

The Quality Assurance process focused primarily on validating the data integrity between the client (Unity) and the cloud (Firebase Realtime Database), ensuring that session states, scoring, and collection rules were strictly enforced. Testing was conducted manually on an Android AR-compatible device.

### Manual Scenario Testing


 **Profile Creation & Auth Error**  1. Attempt sign-up with an email already in use. 2. Log in successfully.  **Game:** A user-friendly error message is displayed (e.g., "Email already in use"). **Firebase:** A new user profile structure with all counts initialized to **`0`** is created only upon successful login/signup. 

 **Transaction Safety (Limiter)**  1. Rapidly tap the "Add to Recipe" button for a card (e.g., Flour) 10 times. Required count is 1.  **Firebase:** The `Flour_Ingredient` count must not exceed **`2`**. The transaction prevents spamming and concurrent writes from corrupting the limit. 

 **Stage & Card Validation**  1. Enter the Pineapple Paste scene. 2. Scan an incompatible card (e.g., Flour, which belongs to Biscuit).  **Game:** The invalid card is **blocked** from spawning any prefab. An **Invalid Card Popup** message is displayed. 

 **Completion Trigger & Scoring**  1. Start Hunt. 2. Collect all required items (DB count met). 3. Physically drag the final item into the bowl.  **Firebase:** The specific component's `durationSeconds` is recorded, and `isComplete` flips to **`true`**. **Game:** The completion panel appears **after** the Firebase update succeeds. 

 **Collection Unlock Feedback**  1. Complete the Pineapple Paste hunt (sets `isComplete: true`). 2. Navigate to the Collections page.  **Game:** The corresponding item/card in the collection UI must be visually **unlocked** (GreyLockPanel is inactive). 


## 11. 🖼️ Original Artwork and Assets

| Asset Type | Source / Attribution | Notes |
| :--- | :--- | :--- |
| **3D Models (Original)** | **Original Artwork (Custom-Built):** The **Kele Tin Can Model** was created specifically for this project. | This model was designed and optimized to be low-poly and PBR-ready for efficient rendering in the AR environment, ensuring performance and unique branding. |
| **AR Image Targets (Cards)** | **Original Artwork (Custom-Designed):** All physical ingredient cards used to trigger the AR experience were custom-designed and photographed/rendered. | Custom artwork ensures unique card design for reliable AR tracking and maintains aesthetic consistency with the Kele Shop theme. |

## 12. 🖼️ External Assets and Libraries

| Asset Type | Source / Attribution | Notes |
| :--- | :--- | :--- |
| **3D Models (External)** | **Sketchfab / Unity Asset Store (Attributed):** Individual ingredient models (Pineapple, Flour, Butter, etc.), the Mixing Bowl, and the final Dough/Paste models. | These assets were selected for their quality and optimization to ensure compatibility and visual performance within the AR environment. |
| **Core Libraries** | **Firebase Unity SDK:** Used for Authentication and Realtime Database services. | Essential for secure user management and persistent, cross-session data integrity. |
| **Unity Packages** | **AR Foundation, ARCore XR Plugin, TextMeshPro (TMPro):** | Standard Unity packages used for implementing AR functionality, tracking, and UI text rendering. |





# 💻 Technical Walkthrough & IT Documentation

### 1. 🔑 Key Controls/Hacks 
This section outlines the primary methods for interacting with the Kele AR application.

### **General User Interface (UI) Controls**

| Control | Action/Function |
| :--- | :--- |
| **Single Tap / Press** | Selects menu options, activates buttons (e.g., *Start Game*, *Collect Ingredient*), and confirms choices. |

## 2. 🎮 How to Play / Run the Application

This guide walks you through the step-by-step process of navigating the Kele AR App, from logging in to completing the Treasure Hunt.

### A. Setup & Main Menu Navigation

1.  **Launch & Log In:**
    * Launch the application on your mobile device.
    * Use the authentication screen to **Log In** to your previous account or **Create a New Account**.

2.  **Main Menu Options:**
    * After logging in, you will arrive at the Main Menu, which provides three primary navigation options:
        * **About Us:** View company history, details, and products from KELE.
        * **AR Scanner:** Start the ingredient treasure hunt and scanning process.
        * **Collections:** View your digital collection of scanned ingredient cards and unlocked recipes.

### B. The Treasure Hunt Gameplay

3.  **Initiate Hunt & Timer:**
    * Select the **AR Scanner** button from the Main Menu. You will be directed to the scanning interface.
    * A **stopwatch timer** (Minutes:Seconds) will appear and automatically begin counting down the moment the hunt initiates.
    * **Note:** If you exit the app or lock your phone, the timer will **reset** and the hunt must be restarted upon resuming the game.

4.  **Scanning & Collecting Ingredients:**
    * Use the device camera to physically locate and scan the **hidden ingredient cards** (AR targets) in the real-life environment.
    * After successfully scanning a card, tap the **"Add to Recipe"** button that appears on the screen to collect the ingredient and add it to your current digital recipe.

5.  **Component Completion & Progression:**
    * Once you have collected all the necessary ingredients for the current **recipe component** (e.g., the Pineapple Paste), a virtual **bowl** will appear.
    * Place all collected ingredients into the bowl to complete that component.
    * Upon successfully completing all recipe components of the treasure hunt, the timer will automatically **stop**, and your total completion time will be recorded.
    * The app will automatically progress you to the next recipe component, repeating the card scanning process.

6.  **Final Completion & Reward:**
    * A **Surprise** button will appear, which provides instructions for collecting a **physical gift** from the store clerk.

### C. Post-Game Features

7.  **Reviewing Collections:**
    * From the Main Menu, select **Collections** to view all your unlocked progress, including the individual ingredient cards and the final completion card.

8.  **Replay for Best Time:**
    * Players can restart the Treasure Hunt at any time from the Main Menu to attempt to achieve a faster total completion time.



### 2. 🖥️ Platforms and Hardware Requirements

The Kele AR application is an **AR-Required** application designed for the Android ecosystem. It leverages Google's ARCore platform for all Augmented Reality functionality, including card scanning and 3D object rendering.

| Category | Requirement / Specification | Notes |
| :--- | :--- | :--- |
| **Platform** | Android OS Only | Currently, the application is exclusive to Android devices. |
| **Minimum OS** | Android 7.0 (API Level 24) or Newer | This is the minimum required OS level for ARCore functionality. |
| **AR Framework** | Google Play Services for AR (ARCore) | The device **must** be ARCore-certified. If the service is not installed, the application will prompt the user for installation/update. |
| **Connectivity** | Internet Connection for Initial Download & Updates | An internet connection (Wi-Fi or Cellular Data) is required for app installation and any necessary ARCore service updates. Gameplay may be functional offline once resources are loaded. |


### 3. 🐛 Limitations and Known Bugs

## 📚 References and Citations (Both DDA and ITD)

The following sources, assets, and documentation were utilized for the research, development, and artistic inspiration of the AR Pastry Chef application.

**Source:** https://www.youtube.com/watch?v=A0nh0huo6ds&t=25s - Easter Egg Hunt AR game - Game reference
**Source:** https://www.8thwall.com/prakria/treasurehunt - Prakria treasure hunt Ar game - Game referenc
**Source:** https://sketchfab.com/3d-models/lemon-ab2625342f2c43fe8a383f8f9b4917ab - Lemon 3D model - Models used in game
**Source:** https://sketchfab.com/3d-models/water-drinking-pool-85565ec5894e4fcc8e0f2fe363468b94 - Water 3D model - Models used in game
**Source:** https://sketchfab.com/3d-models/flour-bag-26fee1793f60423392aac461d4ce4f79 - Flour 3D model - Models used in game
**Source:** https://sketchfab.com/3d-models/sugar-cubes-c47bbf11407043b6938fa638a68c2851 - Sugar 3D model - Models used in game
**Source:** https://sketchfab.com/3d-models/juicy-pineapple-488a3bb858eb4929be657ed74f2d04c5 - Pineapple 3D model - Models used in game
**Source:** https://sketchfab.com/3d-models/five-laps-at-freddys-exotic-butters-6689e0683dd3445b90291804caa55047 - Butter 3D model - Models used in game
**Source:** https://www.kele.sg/pages/our-story - Kele Website - Details for in game infomation
**Source:** https://www.google.com/url?sa=t&rct=j&q=&esrc=s&source=web&cd=&cad=rja&uact=8&ved=2ahUKEwjQ3Mbj_LmRAxUUyzgGHUtGMisQFnoECA0QAQ&url=https%3A%2F%2Fgemini.google.com%2F&usg=AOvVaw0NAGHCxxuvW-RqAT62pGZI&opi=89978449 - Geminin - Research and renfinement
**Source:** https://www.google.com/url?sa=t&rct=j&q=&esrc=s&source=web&cd=&cad=rja&uact=8&ved=2ahUKEwidlr2e_bmRAxU7YfUHHZLlJLoQFnoECA0QAQ&url=https%3A%2F%2Fchatgpt.com%2F&usg=AOvVaw29vbCnS_7PDD4xupasoOfg&opi=89978449 - ChatGpt - Research and renfinement