# The Little Black Book (In The Dark Edition) 🌑👙

This book contains the secrets of the Cleo codebase—specifically, the "The Truth" about the lines of code that remain uncovered by tests. We have purged all "Stupid Defensive" code. What remains is essential, intentional, and justified.

## Cleo.Core (99.0% Coverage) 💎

### 1. `SessionActivity` (87.5%)
*   **Uncovered**: `public virtual string GetMetaDetail() => $"Originator: {Originator} | Evidence: {Evidence.Count}";` (Base Implementation)
*   **The Truth**: This base implementation is a fallback. In practice, all concrete subclasses (e.g., `PlanningActivity`) override this method to provide specific details (e.g., `Steps.Count`). Testing this base method would require instantiating a raw `SessionActivity` mock or a test-specific subclass just to assert this string format. Given that `Originator` and `Evidence` are tested elsewhere, this single line of fallback logic is low-risk noise.

### 2. `RefreshPulseResponse` (66.6%)
*   **Uncovered**: Secondary Constructor / Property Getters (`IsCached`, `Warning`, etc.) not used in happy path tests.
*   **The Truth**: This DTO is a data carrier. Our tests verify the properties relevant to the Use Case logic (e.g., `Status`, `Id`). Writing a test that simply instantiates this DTO and asserts `response.Warning == "warn"` without a corresponding business logic driver is "Test Obsession". We test the logic that *produces* the response, not the auto-generated property getters of the response itself.

### 3. `ViewPlanResponse` (85.7%)
*   **Uncovered**: `Equals`, `GetHashCode`, or secondary properties.
*   **The Truth**: Similar to `RefreshPulseResponse`, this is a DTO. The core path is covered. The remaining gaps are C# record artifacts or unused properties that exist for future API compatibility.

## Conclusion 🏁

The Core handles the Dirty Reality of the world.
*   `WTF` is back to handle the unknown. 🚨
*   `StateUnspecified` and `Paused` are recognized and mapped. 🐤
*   `Stance.Paused` is distinct and results in `DeliveryStatus.Stalled`. 🛑
*   The logic is robust, tested, and pure.

**Confidence Level**: Absolute. 💯
