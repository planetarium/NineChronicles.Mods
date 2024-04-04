#!/bin/bash

dotenvPath="./.env.xml"
configJsonRelativePath="../../config.json"

function get_ninechronicles_dir_from_env() {
    nineChroniclesDir=$(xmllint --xpath 'string(//Project/PropertyGroup/NINECHRONICLES_DIR)' "$dotenvPath")
    if [ -z "$nineChroniclesDir" ]; then
        echo "NINECHRONICLES_DIR not found in $dotenvPath"
        exit 1
    fi
    echo $nineChroniclesDir
}

function create_clo_json() {
    local nineChroniclesDir="$1"
    local configJsonPath="$2"

    planet=$(jq -r '.Planet' "$configJsonPath")
    planetRegistryUrl=$(jq -r '.PlanetRegistryUrl' "$configJsonPath")

    planetData=$(curl -s "$planetRegistryUrl")

    rpcServerHost=$(echo "$planetData" | jq -r --arg planet "$planet" '.[] | select(.id == $planet) | .rpcEndpoints["headless.grpc"][0]')
    rpcServerPort=$(echo "$rpcServerHost" | cut -d ':' -f3)
    rpcServerHost=$(echo "$rpcServerHost" | cut -d ':' -f2 | sed 's/\/\///')
    genesisBlockPath=$(echo "$planetData" | jq -r --arg planet "$planet" '.[] | select(.id == $planet) | .genesisUri')
    apiServerHost=$(echo "$planetData" | jq -r --arg planet "$planet" '.[] | select(.id == $planet) | .rpcEndpoints["dp.gql"][0]')
    marketServiceUrl=$(echo "$planetData" | jq -r --arg planet "$planet" '.[] | select(.id == $planet) | .rpcEndpoints["market.rest"][0]')
    patrolRewardServiceUrl=$(echo "$planetData" | jq -r --arg planet "$planet" '.[] | select(.id == $planet) | .rpcEndpoints["patrol-reward.gql"][0]')
    seasonPassServiceUrl=$(jq -r '.SeasonPassServiceUrl' "$configJsonPath")
    IAPServiceHostUrl=$(jq -r '.IAPServiceHostUrl' "$configJsonPath")
    meadPledgePortalUrl=$(jq -r '.MeadPledgePortalUrl' "$configJsonPath")

    cat << EOF > clo.json
{
    "RpcClient": true,
    "RpcServerHost": "$rpcServerHost",
    "RpcServerPort": $rpcServerPort,
    "SelectedPlanetId": "$planet",
    "MarketServiceUrl": "$marketServiceUrl",
    "AppProtocolVersion": "$(jq -r '.AppProtocolVersion' "$configJsonPath")",
    "OnboardingPortalUrl": "$(jq -r '.OnboardingPortalUrl[0]' "$configJsonPath")",
    "PatrolRewardServiceUrl": "$patrolRewardServiceUrl",
    "SeasonPassServiceUrl": "$seasonPassServiceUrl",
    "iAPServiceHostUrl": "$IAPServiceHostUrl",
    "MeadPledgePortalUrl": "$meadPledgePortalUrl",
    "GenesisBlockPath": "$genesisBlockPath",
    "ApiServerHost": "$apiServerHost"
}
EOF
}

function main() {
    nineChroniclesDir=$(get_ninechronicles_dir_from_env)
    configJsonPath="$nineChroniclesDir/$configJsonRelativePath"
    
    create_clo_json "$nineChroniclesDir" "$configJsonPath"
    
    cp clo.json "$nineChroniclesDir/NineChronicles.app/Contents/Resources/Data/StreamingAssets/clo.json"
    rm clo.json
    
    (cd "$nineChroniclesDir" && ./run_bepinex.sh)
}

main
