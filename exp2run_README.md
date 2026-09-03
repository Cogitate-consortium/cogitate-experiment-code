# Cogitate Experiment 2 Run Code

This part of the repo contains the **compiled Windows build** — everything needed to run the experiment is self-contained here. 

## The task

Participants control an orb at the bottom of the screen and collect falling circles matching its color while avoiding the opposite color. The session is split into "worlds" of four levels each; difficulty adapts to performance. Task-irrelevant images (faces, objects) appear in the background, and the game occasionally pauses to probe whether the participant saw
one. 

Separate **replay levels** replay previously played game levels; there, the participant views their own session, and reports the background stimuli (go/no-go on faces vs. objects). Replay levels only work correctly after the corresponding game level has been played.

## Requirements

- Windows (64-bit)
- Unzip / clone to a **short path** (e.g. the Desktop). Long paths can break logfile saving
- Enough free disk space — logfiles are written inside this folder

## Repository structure

```
Seattle Project (Release)/
├── Orange & Blue - A Tale of Falling Essences.exe   ← launch this
├── UnityPlayer.dll, UnityCrashHandler64.exe, MonoBleedingEdge/   ← Unity runtime
└── Orange & Blue - A Tale of Falling Essences_Data/
    ├── Managed/, Plugins/, Resources/, level*, *.assets   ← game binaries & assets
    └── StreamingAssets/          ← everything an experimenter needs
        ├── Config/               ← config.json (+ one subfolder per lab)
        ├── Content/              ← stimuli (faces, objects, blobs), audio, tutorial slides
        ├── Sequences/            ← per-method trial plans (see below)
        ├── TestAudio/            ← sound files for audio-trigger checks
        ├── _Info/Key Codes.rtf   ← keyboard-key → key-code mapping (reference only)
        └── Logs/                 ← created at runtime; participant data lands here
```

### Sequences

`Sequences/` holds one folder per method/mode (`MEEG`, `ECOG`, `FMRI_Scanner`, `*_Preparation`, `*_Screening`). 
Inside each is a numbered set of pre-made session plans; the one used is picked automatically from the participant ID (participant code modulo the number of sequence folders). 

Each plan contains three CSVs:

- `StimSequence.csv` — which stimulus appears in which world/level/location, and whether it is probed
- `Timings.csv` — when background events and stimuli occur
- `Localizers.csv` — which game levels are replayed as replay levels, and the response type required

### Config

`StreamingAssets/Config/` contains a `config.json` plus one subfolder per participating lab. 
The confid that will be run is the `config.json` file that is in the main `Config/` folder. It sets the lab code, method (`module`: MEEG = 0, ECOG = 1, fMRI = 2), input device / button mapping, triggers, and other experimental parameters.


## Running

1. If using EyeLink, start the eye tracker first and leave the host PC in **Camera Setup** mode.
2. Run `Orange & Blue - A Tale of Falling Essences.exe`. A Unity dialog asks for resolution, display and graphics quality — set, then click **Play**.
3. **Game mode** (fMRI/MEEG only; ECOG goes straight to the next step):
   - `SCREENING` — behavioral pre-screening: the three preparatory levels plus one world (was used to pre-filter participants for taking part in the experiment)
   - `PREPARATION` — the three preparatory levels plus a single run, on the full setup (was used as an in-scanner practice prior to the experimental run)
   - `FULL GAME` — two preparatory levels, all worlds, and the replay levels
4. **Subject info**: lab code + 3-digit participant number, and a run ID. The participant number determines both the button assignment (even = normal start, odd = flipped start) and which sequence folder is used. 
Use one participant code per participant and a new run ID for each mode or restart; on a repeated participant code, choose **RESUME** and a new run number.
For a quick test, use the highlighted "skip and proceed to the game" link, which generates a random ID.
5. On the level map, levels unlock as they are completed.


## Output

Data is written to `StreamingAssets/Logs/<labcode+subject number>/<run ID>/` — one folder per participant, one subfolder per run.

## Handy keys

| Key | Effect |
| --- | --- |
| Right-click a level | Unlock it (play the game level first if you want its replay level) |
| `e` | Advance to the next level from the home screen |
| `p` | Pause the current level |
| `esc` | Leave a level; from the main screen, quit the game |
| `enter` | Confirm the exit dialog if the mouse is unresponsive |
| `Alt`+`F4` | Force quit if frozen (current level's data may be lost) |

With `allowExperimenterKeyCodesInRelease` set to `true` in the config:

| Key | Effect |
| --- | --- |
| `F5` / `F6` | Lock / unlock the navigation screen (e.g. between MEEG recording runs) |
| `F9` | Skip to the end of a level (also needs `EXPERIMENTER_ALLOW_COMPLETE_LEVELS: true`) |
