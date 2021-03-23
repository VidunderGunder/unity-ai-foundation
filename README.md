## Installation

Install [Unity Hub](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe)

Download [repo as ZIP](https://github.com/VidunderGunder/unity-ai-foundation/archive/refs/heads/master.zip) and unzip

Open project through Unity Hub with recommended Unity version

## Troubleshooting

See [ML-Agents docs](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Installation.md) if training isn't working correctly. You might be missing some dependencies, like:

- [Python](https://www.python.org/downloads/)  
*We prefer the [latest version of 3.7](https://www.python.org/downloads/release/python-3710/), as it is the least error prone version as of late*
- [PyTorch](https://pytorch.org/get-started/locally/)  
*Note that CUDA is optional, but nice to have if you want to utilize your GPU.*
- [ML-Agents python package](https://pypi.org/project/mlagents/):  
`pip install mlagents`
- [Visual C++ Redistributable](https://support.microsoft.com/en-us/topic/the-latest-supported-visual-c-downloads-2647da03-1eea-4433-9aff-95f26a218cc0)