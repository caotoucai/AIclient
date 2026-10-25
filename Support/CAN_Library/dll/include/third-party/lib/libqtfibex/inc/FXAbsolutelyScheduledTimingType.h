#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType ABSOLUTELY-SCHEDULED-TIMING-TYPE
 *
 * Content model for the entity TIMING in the absolutely scheduled peculiarity.
 */
class ASAM_FIBEX_EXPORT FXAbsolutelyScheduledTimingType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXAbsolutelyScheduledTimingType(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @brief element DESC */
    QString m_desc;

    /**
     * @brief element SLOT-ID
     *
     * Identifier for the (fixed) slot where the frame described by this timing is sent in the static part of a application cycle.
     * Used as priority for arbitration in the dynamic part of a cycle.
     * Usualy used (with cycleCounter) for frame identification
     */
    qint32 m_slotId;

    /**
     * @brief element CYCLE-COUNTER
     *
     * The communication cycle where the frame described by this timing is sent.
     * If a timing is given in this way the referencing cluster must specify the NUMBER-OF-CYCLES as upper bound and point of total repetition.
     */
    qint16 m_cycleCounter;

    /**
     * @brief element BASE-CYCLE
     *
     * The first communication cycle where the frame described by this timing is sent
     */
    qint16 m_baseCycle;

    /**
     * @brief element CYCLE-REPETITION
     *
     * The number of communication cycles (after the frist cycle) whenever the frame described by this timing is sent again
     */
    qint16 m_cycleRepetition;

    /**
     * @brief element MANUFACTURER-EXTENSION
     * @todo to be implemented
     *
     * Reserved space providing schema extendibility.
     */
};

} // FIBEX
} // ASAM
