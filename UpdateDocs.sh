#First we note the current commit
COMMIT="$(git rev-parse HEAD)"

#then we get the website repo
cd ..
git clone --depth 1 https://github.com/ForLoveOfCats/SiteOfSteel.git
cd SkyOfSteel

#then we build the docs
doxygen

#almost done, next we copy the results into the site repo
cp -a -r "./DoxygenOutput" "../SiteOfSteel//Docs/Master/"

#finally we commit and push
cd ../SiteOfSteel
git add * #track and stage any new files
git stage * #stage any changed files
git commit -m "Update docs for $COMMIT"
git push https://${GH_TOKEN}@github.com/ForLoveOfCats/SiteOfSteel.git

#then just as some cleanup we cd back into the original
#just in case this is being run locally
cd ../SkyOfSteel
