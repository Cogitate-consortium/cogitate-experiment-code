# Experiment 2 Unity Source Code

Unity source project for Cogitate's Experiment 2. It supports MEEG, ECOG, and fMRI setups, with eye tracking.

This is the **source** (built with **Unity 2018.4.22f1**); the compiled Windows build is in a separate folder in this repo.

## The task

Participants control an orb at the bottom of the screen and collect falling circles matching its color while avoiding the opposite color. The session is split into "worlds" of four levels each; difficulty adapts to performance. Task-irrelevant images (faces, objects) appear in the background, and the game occasionally pauses to probe whether the participant saw
one. 

Separate **replay levels** replay previously played game levels; there, the participant views their own session, and reports the background stimuli (go/no-go on faces vs. objects). Replay levels only work correctly after the corresponding game level has been played.

## Opening the project

1. Install **Unity 2018.4.22f1**.
2. Open this folder as a Unity project. Only `Assets/` and `ProjectSettings/` are tracked — Unity regenerates `Library/` and the other generated folders on first open, which takes a while.
3. The build target is Windows (standalone, x64). Several plugins are Windows-only native DLLs.

`Assets/Scenes/Preloader.unity` is the first scene in the build; `MainMenu`, `LevelSelection`, `Game_Runner_2D_Blue`, `Game_Runner_2D_Orange`, `LoadingScreen`, and `FadeScene` follow.

## Structure

```
Assets/
├── Scenes/          Preloader, MainMenu, LevelSelection, Game_Runner_2D_Blue/Orange,
│                    LoadingScreen, FadeScene (in the build) plus standalone test/tool scenes:
│                    LPT_Test, Serial_Test, HeatmapGenerator_Test, FullLogAnalyzer
├── Scripts/         all game and experiment C# code (see below)
├── TGP/             small general-purpose helper library (collections, math, input, editor tools)
├── Prefabs/         managers (0.TheManager), eye tracker prefabs, game, menus, UI, particles
├── Art/, Fonts/, Shaders/, Resources/    visual assets, audio mixer
├── Audio/           music and SFX, organized per world (see Audio/Music Copyright.txt)
├── Plugins/         native/third-party: EyeLink wrapper + Interop.SREYELINKLib,
│                    InpOut (parallel-port access), Koreographer
├── TobiiPro/        Tobii Pro Unity SDK
├── EyeLink/         EyeLink demo scene
├── TextMesh Pro/    TMP package assets
├── StreamingAssets/ runtime-editable experiment data — copied verbatim into the build
└── _Extras (unspecified)/   placeholder folders for labs without a finalized config
ProjectSettings/     Unity project settings, including ProjectVersion.txt
```

### `Assets/Scripts/`

- **`Experiment/`** — everything experiment-side: `ExperimentManagerApplication` / `ExperimentManagerSession` / `ExperimentManagerLevel`, config and path resolution (`LibraryManager/`), stimulus queue generation and reading (`Stimulus/`, with separate helpers for fMRI and MEEG/ECOG), the probe and report tools plus task-relevant/irrelevant stimulus handling (`Task/`), the rotating-squares background (`Background/`), participant info and subject-entry UI (`Subject/`), per-level and per-session analytics (`Analytics/`), the trigger system (`Triggers/`, with LPT, audio, photodiode, and eye-tracking backends plus the trigger code tables), and the offline log analyzer (`Analyzer/`).
- **`Game/`** — the game itself: player controller/model/view, interactables, level managers (worlds, the T/I/P tutorial levels, and localizer/replay levels), session managers (level library, progression, score), and systems for adaptive difficulty, replay, scoring, and cameras.
- **`Peripherals/`** — hardware and I/O: EyeLink and Tobii eye tracking, LPT (parallel port), serial port, low-level WAV audio playback, Koreographer wrapper, high-accuracy USB/serial input, timing, and buffered background logging.
- **`Helpers/`** — shared UI, menu, engine, and async utilities.

Folders named `_UNUSED`, `__Test`, or similar hold retired or test-only code.

### `Assets/StreamingAssets/`

Copied as-is into the build, and the only part experimenters normally touch:

- `Config/` — `config.json` plus one subfolder per lab. The active config is the single `config.json` in the root of `Config/`; a lab uses its own by replacing that file with the one from its subfolder. It defines the lab code, method (`module`: MEEG = 0, ECOG = 1, fMRI = 2), input device and button mapping, trigger settings, and experimental parameters.
- `Sequences/` — one folder per method/mode (`MEEG`, `ECOG`, `FMRI_Scanner`, `*_Preparation`, `*_Screening`), each containing numbered session plans. The plan used is selected from the participant number (participant number modulo the number of plan folders). Each plan holds `StimSequence.csv` (which stimulus appears where, and whether it is probed), `Timings.csv` (event and stimulus times), and `Localizers.csv` (which levels are replayed, and the response type required).
- `Content/` — stimuli (`Faces`, `Objects`, `Blobs`), audio, main-menu images, and tutorial slides per method.
- `TestAudio/` — sound files for audio-trigger checks.
- `_Info/Key Codes.rtf` — keyboard key → key-code reference for the config file.

At runtime, the build also creates `StreamingAssets/Logs/<lab code + participant number>/<run ID>/` next to these folders. Logs are not part of the source tree.

## Credits and licensing

Project company name is Reed College. Music is by Kevin MacLeod (incompetech.com), licensed under Creative Commons Attribution 3.0 — see `Assets/Audio/Music Copyright.txt`. Third-party plugins (Tobii Pro SDK, Koreographer, InpOut, EyeLink) carry their own licenses in their respective folders.
