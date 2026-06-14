# AR Face Filter Studio

A polished **Unity AR Face Filter** app built on **AR Foundation** (ARKit / ARCore).
Try on glasses, sunglasses, masks, party hats, mustaches, floating hearts and a glowing
halo in real time, snap a photo, and share it — all wrapped in a modern, reskinned UI.

> Built for the *Create with AR Face Filters* Unity Learn path. Target editor: **Unity 2022.3 LTS**.

---

## Features

| Area | What's included |
| --- | --- |
| **Reskin / UI-UX** | Modern dark "glassmorphism" theme, custom rounded buttons & cards, consistent palette/typography, animated scene transitions — all driven from a single `AppTheme`. |
| **8 face filters** | Glasses, Sunglasses, Mask, Party Hat, Mustache, Heart stickers (animated), Halo (animated) + None. All generated procedurally, so **no art import is required**. |
| **Toggle filters** | Turn filters on/off at runtime with one tap. |
| **Filter carousel** | Cycle next/previous; last-used filter is remembered between sessions. |
| **Camera switching** | Switch front (user) / back (world) camera where the device supports it. |
| **Screenshot capture** | One-tap shutter with flash; UI is hidden from the shot. |
| **Save & share** | Saves a PNG to the device gallery (Android) and shares via the native share sheet. |
| **Settings panel** | Render quality (Performance / Balanced / Quality), mirror preview, reset filters, reset all. |
| **Demo Mode** | Hands-free auto-cycling scene with big captions + progress bar — perfect for recording the demo video. Tap to pause. |

---

## Project structure

```
Assets/
├── Scenes/        MainMenu.unity · ARFaceFilter.unity · Demo.unity
├── Scripts/
│   ├── Core/      AppTheme · UIFactory · AppSettings · SceneNavigator · FadeTransition
│   ├── AR/        ARRigBuilder · FaceFilterManager · FilterContentFactory ·
│   │              FilterCatalog · FilterSpin · ARCameraSwitcher
│   ├── Capture/   ScreenshotCapture · NativeShare
│   └── UI/        MainMenuController · ARFilterUIController · SettingsPanel ·
│                  SharePreview · DemoModeController
├── Prefabs/  ·  UI/  ·  Materials/  ·  Resources/
Packages/manifest.json     (AR Foundation 5.1, ARKit, ARCore, XR plug-ins)
ProjectSettings/           (Unity version + build scene list)
```

### Architecture note
Each scene asset contains a **single bootstrap GameObject**; the AR rig and the entire
themed UI are constructed at runtime from code (`AppRig`, `UIFactory`, the UI
controllers). This keeps scene files tiny and guarantees the rig is always wired
correctly — no fragile inspector references to break.

### Simulation Mode (record your demo with no phone)
`AppRig` automatically picks the rig for the platform:
- **Android / iOS build** → real AR Foundation face tracking (`ARRigBuilder` + `FaceFilterManager`).
- **Unity Editor / desktop / WebGL** → hardware-free **Simulation Mode**
  (`SimulatedFilterController` + a procedural mannequin head that gently turns), so the
  full app — menu, filters, carousel, toggle, capture, settings, demo — is fully usable
  and **recordable straight from the Editor's Game view**. No ARKit/ARCore device required.

Press **Play** on `MainMenu.unity`, hit **Start AR Filters** (or **Demo Mode**), and you'll
see filters on the mannequin immediately. Set `AppRig.ForceSimulation = true` to use the
mannequin on a device too.

---

## Open & run (step-by-step)

1. **Install Unity 2022.3 LTS** (e.g. `2022.3.40f1`) via Unity Hub, with the
   **Android Build Support** (SDK/NDK/JDK) and/or **iOS Build Support** modules.
2. **Open the project**: Unity Hub → *Add* → select this folder → open. Unity will
   resolve the AR Foundation packages from `Packages/manifest.json` automatically.
3. Open `Assets/Scenes/MainMenu.unity` and press **Play** to see the menu/UI in the
   Editor. (Live face tracking needs a device — see below.)

### Enable XR providers (one-time, required for on-device AR)
> This is the only manual setup step — it must be done in the Editor because it writes
> XR-loader assets.

- **Edit → Project Settings → XR Plug-in Management** → *Install* if prompted.
  - **Android tab** → enable **Google ARCore**.
  - **iOS tab** → enable **Apple ARKit**.
- **Project Settings → Player**:
  - *Other Settings* → set **Camera Usage Description** (e.g. "Used for AR face filters").
  - Android: **Minimum API Level 24+**, **Scripting Backend = IL2CPP**, **ARM64** only,
    and **uncheck** *Auto Graphics API* leaving **OpenGLES3 / Vulkan**.
  - iOS: **Target minimum iOS 13+**, **Requires ARKit support** as needed.

### Build to a device
- **Android (ARCore-supported phone)**: `File → Build Settings → Android → Switch
  Platform → Build And Run`.
- **iOS (iPhone with TrueDepth/ARKit)**: switch platform to iOS, Build, then open the
  Xcode project, sign with your team and run.

The build scene list is already set: **MainMenu → ARFaceFilter → Demo**.

---

## Recording the demo video
1. Build & run on a device (or use Demo Mode in the Editor's Game view for a UI-only walkthrough).
2. From the menu choose **Demo Mode** — filters auto-cycle with captions; great for a clean recording.
3. Or use **Start AR Filters** and manually show: carousel → toggle → camera switch →
   capture → share → settings.

---

## Known limitations / missing components (by design)
- **Live face tracking only runs on a real ARKit/ARCore device.** In the Editor the UI,
  menus, transitions, demo flow and capture work, but no face is tracked (you can add
  *XR Simulation* / *AR Foundation Remote* if you want in-Editor faces).
- **iOS share** uses an Editor/console fallback; wire a native share plugin (or the
  Android path already provided) for production iOS sharing.
- Android gallery save targets `DCIM/FaceFilter` and notifies the media scanner; on
  Android 11+ you may prefer the MediaStore API for scoped storage.
- Filters are **procedural primitives** (no imported 3D art) so the project runs with
  zero external assets — swap in your own models in `FilterContentFactory` for richer looks.

---

## Where to customize
- **Theme / reskin** → `Assets/Scripts/Core/AppTheme.cs`
- **Add a filter** → add a `FilterType` + builder in `FilterContentFactory.cs`, then a
  `FilterEntry` in `FilterCatalog.cs`.
- **Demo timing** → `DemoModeController.secondsPerFilter`.
