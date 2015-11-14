KSPDIR		:= ${HOME}/ksp/KSP_linux
GAMEDATA	:= ${KSPDIR}/GameData
STGAMEDATA  := ${GAMEDATA}/SurveyTransponder

RESGEN2		:= resgen2
GMCS		:= gmcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

ICONS=\
	icon_tgt_targeted.png \
	icon_tgt_idle.png \
	$e

all: ${ICONS}

info:
	@echo "Survey Transponder Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

icon_tgt_targeted.png: icon_target.svg
	sed -e 's/\#ffffff/\#ff48ff/g' $^ | inkscape --export-width=16 --export-height=16 --export-background='#000000' --export-background-opacity=0 --export-png=$@ /dev/stdin

icon_tgt_idle.png: icon_target.svg
	inkscape --export-width=16 --export-height=16 --export-background='#000000' --export-background-opacity=0 --export-png=$@ $^

clean:
	rm -f ${ICONS}

install: all
	mkdir -p ${STGAMEDATA}/Textures
	cp ${ICONS} ${STGAMEDATA}/Textures

.PHONY: all clean install
