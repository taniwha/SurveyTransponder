KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
STGAMEDATA  := ${GAMEDATA}/SurveyTransponder
PLUGINDIR	:= ${STGAMEDATA}/Plugins

TARGETS		:= SurveyTransponder.dll

ST_FILES := \
	AllowWater.cs \
	AssemblyInfo.cs	\
	RenameWindow.cs \
	StagedAnimation.cs \
	Transponder.cs \
	Tracker.cs \
	VersionReport.cs \
	$e

DOC_FILES := \
	../License.txt \
	$e

#	SurveyTransponder.png
#	README.md

RESGEN2		:= resgen2
GMCS		:= mcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: version ${TARGETS} #SurveyTransponder.png

.PHONY: version
version:
	@./git-version.sh

info:
	@echo "SurveyTransponder Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

SurveyTransponder.dll: ${ST_FILES}
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-r:KSPUtil \
		-out:$@ $^

#SurveyTransponder.png: SurveyTransponder.svg
#	inkscape --export-png $@ $^

clean:
	rm -f ${TARGETS} AssemblyInfo.cs #SurveyTransponder.png

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
	cp ${DOC_FILES} ${STGAMEDATA}

.PHONY: all clean install
