# Armageddon Gifs Component

Project: `Worms.Armageddon.Gifs`

Generates GIF animations from Worms Armageddon replays.

## GifCreator

```csharp
Task<string> CreateGif(
    string replayPath,
    TimeSpan turnStart,
    TimeSpan turnEnd,
    int turnNumber,
    string outputFolder,
    uint framesPerSecond = 5,
    uint speedMultiplier = 2,
    TimeSpan? startOffset = null,
    TimeSpan? endOffset = null)
```

Returns the filename (not the full path) of the generated GIF.

### Process

1. Computes `framesFolder` from `GameInfo.CaptureFolder` (where WA deposits extracted PNGs)
2. Deletes any existing frames in that folder
3. Calls `IWormsArmageddon.ExtractReplayFrames(...)` — this runs WA with `/getvideo` to dump PNG frames
4. Assembles frames into a GIF using **ImageMagick** (`MagickImageCollection`)
5. Quantises to 256 colours and optimises transparency
6. Deletes the frame folder

### GIF settings

- Frames are resized to 640×480
- `animationDelay = 100 / framesPerSecond / speedMultiplier` (in hundredths of a second, ImageMagick units)
- Default: 5 fps × 2× speed = 10 fps effective playback

## Dependencies

- `Magick.NET-Q8-AnyCPU` (ImageMagick .NET binding) — used for frame assembly
- `IWormsArmageddon` — for frame extraction
- `IFileSystem` (System.IO.Abstractions) — for testable file operations

## Turn selection (in WA Runner)

`GifCreator` itself only assembles a GIF from given time bounds. Turn selection logic lives in `Worms.Hub.Armageddon.Runner/Processor.cs`:

- Finds the turn with the maximum total damage (`Damage.Sum(d => d.HealthLost)`)
- GIF starts 3 seconds before the first weapon fired in that turn (clamped to turn start)
- GIF ends at turn end
- If GIF creation fails, the error is logged and processing continues (non-fatal)
