# Code Review Standards
In this document we are going to explore the main things we look for in a code review, and what do they mean. If you do not have a lot of familiarity with code reviews please check out the following links before proceeding:

* https://www.ideamotive.co/blog/code-review-best-practices
* https://github.com/google/eng-practices

## What to look for?

Aside of general review guidelines that are applied everywhere, here's a list of needs that are addressed by code reviews and are particularly valued by the Decentraland collaborators. 

### Ensure the general approach of the PR is the proper one
The reviewer has to ensure that the general approach of the intended change is the proper one. 

Sometimes, the best solution isn’t viable because we have urgency or the solution is too expensive to implement right now. If this is the case we generally agree on creating an issue to address the change later as technical debt. 

### Keep our code consistent

#### Code and architecture style

There are many details to look for in a pull request that can't be automated. The contributions are coming from many of us, but the code has to be felt as if it was written by a single person. 

This said, any contribution should be curated using the [coding guidelines](style-guidelines.md) document.

#### Solution consistency

In a large codebase it's usual to stumble upon a same problem twice. Be an algorithmic, API or architectural issue, we have to make sure two equivalent issues have the same solution applied when its sensible to do so. 

This has many benefits, including: 

* **Developer friendly codebase:** if any collaborator already looked at this solution elsewhere, she will already be familiar with it in a new place.

* **High productivity:** we can reach a toolset of well known solutions.

* **High quality solutions:** if many collaborators are looking for consensus on the best solution possible for a particular common problem, the outcome will be as high quality as it can be.   


### Prevent performance and bug regressions

[IBM found](http://www.ifsq.org/work-holland-1999.html) that each hour of code review saves about 100 hours of related work, including QA. After introducing code reviews, [Raytheon reduced](http://www.ifsq.org/finding-idd-7.html) its cost of rework from 40% to 20% of the project cost. The money cost of fixing bugs dropped by 50%. 

We can inspect the code to look for bugs, and we can also:

* **Do smoke testing:** The code may have tests, but is not assured that the tests will cover every use case in a E2E scenario. Run a CI build and give it a whirl.

* **Ensure the code has enough testing coverage:** *All pull requests* that add new features should introduce tests. If the change is a fix, check if a test can be made to cover it.

* **Check API usages are following good practices:** Not keeping good API practices can introduce performance issues down the road. In the worst of cases they can introduce bugs.

### Prevent unnecessary technical debt
#### KISS and good engineering principles
Contributions should follow good engineering principles like KISS, DRY and SOLID when is sensible to do so. Bad practices lead to [come smell](https://refactoring.guru/refactoring/smells). Always remember that code smell [increases the cost](https://martinfowler.com/articles/is-quality-worth-cost.html) of improving any software project, so we should avoid it.

#### If it can't be prevented, let's track it
Sometimes, a request for improvement can't be made because of dangling technical debt that comes from a previous change or time constraints. When this is the case, make sure the issue gets tracked to approach it later.

### Improve transparency of the codebase, raise code standards
By requesting a code review, the knowledge of the changes are shared and the collaborators will stay up to date with the codebase. This is important for our productivity as this enables informed decision making and effective ownership. 

On top of this, good code ideas spread like gospel, raising the quality bar for all the reviewers involved.

Always remember that the greatest changes of the world are brought by collective excellence rather than by isolated achievements.

## Code review process recommendations

### Strive to get reviews as fast as possible

[Data indicates](https://codeclimate.com/blog/virtuous-circle-software-delivery/) that the delay on getting reviews is one of the biggest offenders of software cycle throughput.
*  When work starts on a new branch, the matching pull request should be opened as soon as possible.

*  Review requests should be fired ASAP as well, so don't wait until your code is perfect before asking.

### Avoid big pull requests

Reviews are more effective on small pull requests. [A Cisco study on code reviews](https://static1.smartbear.co/support/media/resources/cc/book/code-review-cisco-case-study.pdf) shows that a single developer can review a limited number of lines of code before losing effectiveness.

* Try to divide the work of big features on smaller pull requests. Plan ahead. Use feature toggling if applicable.

* No pull request can be too small. If a given change has to be a one liner, go for it.

* If everything fails and the PR came out big, bring more reviewers and try to add a guide on the PR description that guides the reviewer through the code. Live meetings to explain the code are very welcome as well. 

* Bonus advice: some auditors use our commit throughput to correlate with MANA performance. So smaller commits indirectly contribute to MANA performance. *(please don't take this too serious)*  

### Bring critical reviewers on board

When working on any existing piece of code, you'd want to get a review the original contributor or owner of the feature. If the repository settings let you merge, that doesn't mean an additional reviewer shouldn't be brought on board.