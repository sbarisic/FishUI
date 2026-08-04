# FishUI 2.0 codebase audit

Baseline: `982a71297434a34157368f9e1ad8065bc4711c62`.

The baseline built successfully and passed 183 tests. Its measured coverage was 39.8% line and 33.1% branch. The 2.0 revision addresses the confirmed runtime defects found during that audit:

- Child paint and pick order were inconsistent, and disabled ancestors could still receive or block input.
- Detached subtrees could retain focus, modal, hover, press, drag, tooltip, overlay, or animation state.
- Dropdown and date-picker overlays used process-wide state and date-picker content was clipped by its owner.
- Layout and animation work happened during drawing, so update-only and draw-only use was nondeterministic.
- Scissor cleanup was not exception-safe and mixed direct/stacked scissoring lost the parent clip.
- Key input consumed only one queued press and touch input did not provide a stable pointer gesture.
- Layout tags were globally mutable and malformed graphs could replace the active UI before validation completed.
- Raylib images, textures, and fonts had no explicit owner or deterministic unload path.
- The sample screenshot helper captured the Windows desktop and introduced a platform-specific System.Drawing dependency.

The revised contracts use explicit UI lifecycle states, update-time layout preparation, per-UI overlays, transactional layout loading, backend-owned immutable resource handles, and bounded input queues. Diagnostics now distinguish manual and effective chart ranges and aggregate drag updates in the human summary while retaining complete events in JSON.

The final automated run passed 223 tests and measured 60.91% line coverage and 47.31% branch coverage. The revised `FishUI` runtime reached 80.28% line and 73.37% branch coverage. The animation, diagnostics, scissor, type-registry, and layout-validation modules also exceeded the 80% line and 70% branch targets when measured by source file.

The Raylib diagnostic sample completed 30-frame runs with both host-owned and backend-owned Raylib frames. Each run produced a complete five-file bundle with an available screenshot and overlay, 1280 by 720 coordinate and framebuffer dimensions, unit X/Y scale, and no diagnostic warnings. This validates capture orientation and resource shutdown at the current desktop scale.

The baseline coverage artifact was generated under `.audit-results`. The directory is intentionally not shipped; this document preserves its result. CI enforces 55% line and 45% branch floors on Windows and Linux and uploads each Cobertura report. Physical high-DPI framebuffer behavior still requires manual runtime checks at 125% and 200% display scaling because CI does not emulate those display modes.
