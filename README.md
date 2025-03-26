# Trump Simulator - Trump Speech AI Simulator

This is a voice simulator featuring Trump, using the generative AI model Qwen2.5 plus a Recorded Grammar Database (RAG) to provide realistic conversations that are converted to Trump's voice playback via ElevenLabs TTS. Users can enter questions to interact with the AI to simulate Trump's style and tone.

---

## Technical Framework

- Unity Engine (version 2022 or above)
- Qwen2.5 model (open source large language model provided by Alibaba)
- RAG: Trump quotes file `trump_quotes.json` as a retrieval generation aid
- ElevenLabs TTS API: convert text to Trump-like style speech
- C# script integration

---------
## Installation and execution

### Installation

1. Open the project folder with Unity 2022+. 2.
2. Ensure that the `Newtonsoft.Json` package is installed (via the Unity Package Manager). 3.
3. Go to the following two files to set whether the API key and voice ID exist:
   - `QwenChat.cs`: Fill in the Qwen2.5 API URL and Bearer Token.
   - `ElevenLabsTTS.cs`: fill in the API Key and VoiceID of ElevenLabs.

### How to run

- Enter the Unity editor and press `Play` to run the simulator.
- Or run the `IIAT460_TrumpSimulator` file in the Build folder and you will see the Unity runtime file named IAT460_Final.

--- --- --- --- --- --- --- --- --- --- --- ---

## Project structure 

```
TrumpSimulator/
├── Assets/ → Unity resources and scripts, including QwenChat.cs, TrumpQuotesDatabase.cs, ElevenLabsTTS.cs, TrumpUIDialog.cs.
├── trump_quotes.json → the Trump Quotes Database used by RAG Search
├── README.md → this description file           
└── IAT460_TrumpSimulator/ IAT460_Final → No need to install the executable file.
```

--- --- --- --- --- --- --- --- --- --- --- ---

## Creator

Che-wei, Lin  
IAT 460 - Spring 2025

---

## 📄 Authorization

This project is intended for IAT 460 coursework and academic use only, and should not be used for commercial purposes.
