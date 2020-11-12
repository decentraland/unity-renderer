import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { LoadingMessage } from "./LoadingMessage";
import { ProgressBar } from "./ProgressBar";

import imgMana from "../../images/loadingtips/Mana.png";
import imgMarketplace from "../../images/loadingtips/Marketplace.png";
import imgLand from "../../images/loadingtips/Land.png";
import imgLandImg from "../../images/loadingtips/LandImg.png";
import imgWearables from "../../images/loadingtips/WearablesImg.png";
import imgDAO from "../../images/loadingtips/DAOImg.png";
import imgGenesisPlazaImg from "../../images/loadingtips/GenesisPlazaImg.png";

export type LoadingTip = {
  text: string;
  image: string;
};

export const loadingTips: Array<LoadingTip> = [
  {
    text: `MANA is Decentraland’s virtual currency. Use it to buy LAND and other premium items, vote on key policies and pay platform fees.`,
    image: imgMana,
  },
  {
    text: `Buy and sell LAND, Estates, Avatar wearables and names in the Decentraland Marketplace: stocking the very best digital goods and paraphernalia backed by the ethereum blockchain.`,
    image: imgMarketplace,
  },
  {
    text: `Create scenes, artworks, challenges and more, using the simple Builder: an easy drag and drop tool. For more experienced creators, the SDK provides the tools to fill the world with social games and applications.`,
    image: imgLand,
  },
  {
    text: `Decentraland is made up of over 90,000 LANDs: virtual spaces backed by cryptographic tokens. Only LANDowners can determine the content that sits on their LAND.`,
    image: imgLandImg,
  },
  {
    text: `Except for the default set of wearables you get when you start out, each wearable model has a limited supply. The rarest ones can get to be super valuable. You can buy and sell them in the Marketplace.`,
    image: imgWearables,
  },
  {
    text: `Decentraland is the first fully decentralized virtual world. By voting through the DAO  ('Decentralized Autonomous Organization'), you are in control of the policies created to determine how the world behaves.`,
    image: imgDAO,
  },
  {
    text: `Genesis Plaza is built and maintained by the Decentraland Foundation but is still in many ways a community project. Around here you'll find several teleports that can take you directly to special scenes marked as points of interest.`,
    image: imgGenesisPlazaImg,
  },
];

export type LoadingState = {
  status: string;
  helpText: number;
  pendingScenes: number;
  message: string;
  subsystemsLoad: number;
  loadPercentage: number;
  initialLoad: boolean;
  waitingTutorial: boolean;
  showLoadingScreen: boolean;
};
const mapStateToProps = (state: any) => {
  return {
    state: state.loading,
    loginStage: state.session.loginStage,
    showWalletPrompt: state.session.showWalletPrompt || false,
  };
};

export interface LoadingContainerProps {
  state: LoadingState;
  loginStage?: string;
  showWalletPrompt: boolean;
}

const changeTip = (current: number) => {
  return current + 1 < loadingTips.length ? current + 1 : 0;
};

export const LoadingContainer: React.FC<LoadingContainerProps> = (props) => {
  const { state, showWalletPrompt } = props;
  const [step, setStep]: [number, any] = useState(0);
  // setting animation of loading
  useEffect(() => {
    const interval = setInterval(() => setStep(changeTip), 5000);
    return () => clearInterval(interval);
  }, []);

  const tip = loadingTips[step];
  const subMessage =
    state.pendingScenes > 0
      ? state.message || "Loading scenes..."
      : state.status;
  const percentage = Math.min(
    state.initialLoad
      ? (state.loadPercentage + state.subsystemsLoad) / 2
      : state.loadPercentage,
    100
  );
  const show = state.showLoadingScreen || state.waitingTutorial;
  const withImages =
    state.initialLoad ||
    props.loginStage !== "completed" ||
    state.waitingTutorial;
  return (
    <React.Fragment>
      {show && (
        <LoadingMessage
          image={withImages ? tip.image : undefined}
          message={withImages ? tip.text : undefined}
          subMessage={subMessage}
          showWalletPrompt={showWalletPrompt}
        />
      )}
      {show && <ProgressBar percentage={percentage} />}
    </React.Fragment>
  );
};

export default connect(mapStateToProps)(LoadingContainer);
