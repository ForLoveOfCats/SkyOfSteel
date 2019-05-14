#First we note the current commit
COMMIT="$(git rev-parse HEAD)"

#then we get the GithubPages branch
mkdir ../GithubPages
cd ../GithubPages
git clone --depth 1 -b GithubPages --single-branch https://github.com/ForLoveOfCats/SkyOfSteel.git
cd SkyOfSteel
git checkout GithubPages #just in case
cd ../../SkyOfSteel

#then we build the docs from the original clone
doxygen

#almost done, next we copy the results into the copy
cp -a -r "./Docs" "../GithubPages/SkyOfSteel/Docs"

#finally we commit and push to the branch
cd ../GithubPages/SkyOfSteel
git add * #track and stage any new files
git stage * #stage any changed files
git commit -m "Update docs for $COMMIT"
git push https://${GH_TOKEN}@github.com/ForLoveOfCats/SkyOfSteel.git

#then just as some cleanup we cd back into the original
#just in case this is being run locally
cd ../../SkyOfSteel
