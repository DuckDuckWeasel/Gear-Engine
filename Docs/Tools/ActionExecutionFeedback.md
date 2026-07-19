# Action Execution Feedback

Block command lists and Action Invoker child lists expose the same runtime feedback.

- Ordered Sequence and Selector lists color the running row blue.
- An action implementing `IActionProgressProvider` can report normalized progress from `0` to `1`; the blue row then fills from left to right.
- Completed rows remain green for success or red for failure.
- Random, Shuffle, Parallel, Parallel Selector, and Utility Selector show a centered message describing the completion rule currently being awaited.
- Lists with one item omit execution feedback because the active item is already unambiguous.

`Wait` implements `IActionProgressProvider` using its configured duration. Actions without a measurable duration remain highlighted while they execute; the editor does not invent progress for them.

Terminal colors are scoped to one execution. Stop, Stop All, Play All, and Play From Selected clear the previous results before another execution begins.

## Validation badges

Action rows use Unity's error and warning icons. The tooltip contains the validation message.

- A missing action reference is an error.
- An `ActionBase.GetSummary()` value beginning with `Error:` produces an error badge.
- A summary beginning with `Warning:` produces a warning badge.
- Any other summary has no badge.

New actions should use these prefixes only for actionable authoring validation. Runtime failures continue to use the action status and logging contracts.

Random weight fields display at most two decimal places. This formatting is presentation-only; automatic balancing retains its full runtime precision.
