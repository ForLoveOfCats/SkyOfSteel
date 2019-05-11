#First we note the current commit
COMMIT="$(git rev-parse HEAD)"

#then we duplicate the entire repo
cp -a -r ../SkyOfSteel ../GithubPages

#then we checkout the correct branch in the copy
cd ../GithubPages
pwd
ls
git fetch
git checkout GithubPages
git reset --hard
git clean -d -f
cd ../SkyOfSteel

#then we build the docs from the original clone
doxygen

#almost done, next we copy the results into the copy
cp -a -r ./Docs ../GithubPages/Docs

#finally we commit and push to the branch
cd ../GithubPages
git add * #track and stage any new files
git stage * #stage any changed files
git commit -m "Update docs for $COMMIT"
git push https://${GH_TOKEN}@github.com/ForLoveOfCats/SkyOfSteel.git

#then just as some cleanup we cd back into the original
#just in case this is being run locally
cd ../SkyOfSteel
